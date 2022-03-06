using CastIt.Domain.Exceptions;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using PCRE;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace CastIt.Youtube
{
    public class YoutubeUrlDecoder : IYoutubeUrlDecoder
    {
        private const int DefaultQuality = 360;
        private const string TitleKeyWord = "<meta property=\"og:title\"";
        private const string DescriptionKeyWord = "<meta name=\"description\"";
        private const string ThumbnailKeyWord = "<meta property=\"og:image\"";
        private const string YoutubeUrl = "https://www.youtube.com";
        private const string ContentValueKeyWord = "content=\"";
        private const string YoutubePlayerConfig = "ytplayer.config";
        private const string UrlEncodedStreamMap = "\"url_encoded_fmt_stream_map\":";
        private const string ConsentRegex = "(consent).*?(youtube).*?(.com)";
        private const string YoutubeRegex = "(youtube).*?(.com)";
        private const string YouTubePlayListPath = "/playlist";
        private const string YoutubePlayListQueryParam = "list";
        private const string YoutubeVideoQueryParam = "v";

        private readonly ILogger<YoutubeUrlDecoder> _logger;
        private readonly JsDescrambler _jsDescrambler;
        private readonly NDescrambler _nDescrambler;

        public YoutubeUrlDecoder(ILogger<YoutubeUrlDecoder> logger)
        {
            _logger = logger;
            _jsDescrambler = new JsDescrambler(logger);
            _nDescrambler = new NDescrambler(logger);
        }

        public bool IsYoutubeUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }
            return Uri.TryCreate(url, UriKind.Absolute, out _) &&
                   (Regex.IsMatch(url, YoutubeRegex) ||
                   Regex.IsMatch(url, ConsentRegex) ||
                   url.Contains("youtu.be"));
        }

        public bool IsPlayList(string url)
        {
            try
            {
                var query = GetQueryParams(url);
                return IsYoutubeUrl(url) && query.AllKeys.Contains(YoutubePlayListQueryParam, StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(IsPlayList)}: Unknown error while parsing url = {url}");
                return false;
            }
        }

        public bool IsPlayListAndVideo(string url)
        {
            var query = GetQueryParams(url);
            return IsPlayList(url) && query.AllKeys.Contains(YoutubeVideoQueryParam, StringComparer.OrdinalIgnoreCase);
        }

        public async Task<BasicYoutubeMedia> ParseBasicInfo(
            string url,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"{nameof(ParseBasicInfo)}: Trying to parse url = {url}");
            if (!IsYoutubeUrl(url))
            {
                _logger.LogWarning($"{nameof(ParseBasicInfo)}: Url = {url} is not a valid youtube url");
                throw new UrlCouldNotBeParsedException(url);
            }

            if (Regex.IsMatch(url, ConsentRegex))
            {
                _logger.LogInformation($"{nameof(ParseBasicInfo)}: Url = {url} is of type consent...");
                string finalUrl = GetQueryParam(url, "continue");
                if (string.IsNullOrWhiteSpace(finalUrl))
                {
                    throw new UrlCouldNotBeParsedException(finalUrl);
                }
                return await ParseBasicInfo(finalUrl, cancellationToken);
            }

            if (!IsHtmlPage(url))
            {
                _logger.LogInformation(
                    $"{nameof(ParseBasicInfo)}: Url is not an html page, " +
                    "checking if we can get the video id...");
                int lasIndex = url.LastIndexOf("/", StringComparison.InvariantCulture);
                if (lasIndex > 0)
                {
                    string videoId = url.Substring(lasIndex + 1);
                    string finalUrl = CreateYoutubeUrl(videoId);
                    return await ParseBasicInfo(finalUrl, cancellationToken);
                }
            }

            string body = await GetContentFromUrl(url, cancellationToken);
            body = body.Replace("\\/", "/");

            VideoQualities qualities = GetVideoQualities(body);
            var (isHls, hlsUrl) = IsHls(body);
            return new BasicYoutubeMedia
            {
                Url = isHls ? hlsUrl : url,
                IsHls = isHls,
                Title = GetVideoKeywordValue(body, TitleKeyWord),
                Description = GetVideoKeywordValue(body, DescriptionKeyWord),
                ThumbnailUrl = GetVideoKeywordValue(body, ThumbnailKeyWord),
                Body = body,
                VideoQualities = qualities
            };
        }

        public async Task<YoutubeMedia> Parse(
            string url,
            int? desiredQuality = null,
            CancellationToken cancellationToken = default)
        {
            var basicInfo = await ParseBasicInfo(url, cancellationToken);
            return await Parse(basicInfo, desiredQuality, cancellationToken);
        }

        public async Task<YoutubeMedia> Parse(
            BasicYoutubeMedia basicInfo,
            int? desiredQuality = null,
            CancellationToken cancellationToken = default)
        {
            desiredQuality ??= DefaultQuality;
            _logger.LogInformation($"{nameof(Parse)}: Trying to parse url = {basicInfo.Url}. Desired quality = {desiredQuality}");

            string fmt = string.Empty;
            if (IsHtmlPage(basicInfo.Url))
            {
                _logger.LogInformation($"{nameof(Parse)}: Url is an html page, trying to retrieve fmt param...");
                fmt = GetQueryParam(basicInfo.Url, "fmt");
            }

            string body = basicInfo.Body;
            var media = new YoutubeMedia(basicInfo);

            _logger.LogInformation($"{nameof(Parse)}: Retrieving js url...");
            string jsUrl = GetJsUrl(body);

            _logger.LogInformation($"{nameof(Parse)}: Retrieving the real url...");
            await GetAndSetRealUrl(media, body, jsUrl, fmt, desiredQuality.Value, cancellationToken);
            return media;
        }

        public async Task<List<string>> ParsePlayList(
            string url,
            CancellationToken cancellationToken = default)
        {
            if (!IsYoutubeUrl(url))
            {
                _logger.LogWarning($"{nameof(ParsePlayList)}: Url = {url} is not a valid youtube url");
                throw new UrlCouldNotBeParsedException(url);
            }
            
            var links = new List<string>();
            _logger.LogInformation($"{nameof(ParsePlayList)}: Parsing url = {url}");

            //Here we got an url like this: https://www.youtube.com/watch?v=somevideoid
            if (IsPlayListAndVideo(url))
            {
                //Here we want an url like this: https://www.youtube.com/playlist?list=someplaylistid
                string playlistId = GetPlayListId(url);
                url = $"{YoutubeUrl}{YouTubePlayListPath}?{YoutubePlayListQueryParam}={playlistId}";
                _logger.LogInformation($"{nameof(ParsePlayList)}: Url is playlist and video, the final url will be = {url}");
            }

            string body = await GetContentFromUrl(url, cancellationToken);
            var html = new HtmlDocument();
            html.LoadHtml(body);
            var root = html.DocumentNode;
            var table = root.Descendants().FirstOrDefault(f => f.Id == "pl-video-table");
            //Sometimes the html returns a json, other times it returns a table
            if (table != null)
            {
                _logger.LogInformation($"{nameof(ParsePlayList)}: Body contains table, parsing it...");
                links = table.Descendants("a")
                    .Where(node => node.HasClass("pl-video-title-link"))
                    .Select(node => RemoveNotNeededParams(YoutubeUrl + node.GetAttributeValue("href", string.Empty)))
                    .Where(link => link.StartsWith(YoutubeUrl, StringComparison.OrdinalIgnoreCase))
                    .Distinct()
                    .ToList();
            }
            else
            {
                _logger.LogInformation($"{nameof(ParsePlayList)}: Body contains pure javascript, parsing it...");
                string pattern = @"(\/watch).*?(?="")";
                body = ReplaceWithAmpersand(body).Replace("\\/", "/");
                links = Regex.Matches(body, pattern)
                    .Select(match => RemoveNotNeededParams(YoutubeUrl + match.Value))
                    .Where(link => link.StartsWith(YoutubeUrl, StringComparison.OrdinalIgnoreCase))
                    .Distinct()
                    .ToList();
            }
            _logger.LogInformation($"{nameof(ParsePlayList)}: Got {links.Count} link(s)");

            return links;
        }

        private (bool, string) IsHls(string body)
        {
            string livePattern = @"(?<=hlsManifestUrl\\"":\\"").*?(?=\\)";
            var liveMatch = Regex.Match(body, livePattern);
            if (!liveMatch.Success)
            {
                livePattern = "(?<=hlsManifestUrl\":\").*?(?=\")";
            }

            liveMatch = Regex.Match(body, livePattern);
            return (liveMatch.Success, liveMatch.Value);
        }

        private bool IsHtmlPage(string url)
        {
            string patternA = @"(\/watch).*?(\?)";
            string patternB = @"(\/live)";
            string patternC = @"(\/live).*?(\?)";

            return Regex.IsMatch(url, patternA) ||
                   Regex.IsMatch(url, patternB) ||
                   Regex.IsMatch(url, patternC);
        }

        private string CreateYoutubeUrl(string videoId)
        {
            return YoutubeUrl + $"/watch?v={videoId}";
        }

        private string GetVideoId(string url)
        {
            var uri = new Uri(url);

            // you can check host here => uri.Host <= "www.youtube.com"
            var query = GetQueryParams(url);

            return query.AllKeys.Contains(YoutubeVideoQueryParam, StringComparer.OrdinalIgnoreCase)
                ? query[YoutubeVideoQueryParam]
                : uri.Segments.Last();
        }

        private string GetPlayListId(string url)
        {
            var query = GetQueryParams(url);
            return query.AllKeys.Contains(YoutubePlayListQueryParam, StringComparer.OrdinalIgnoreCase)
                ? query[YoutubePlayListQueryParam]
                : string.Empty;
        }

        private string RemoveNotNeededParams(string url)
        {
            string videoId = GetVideoId(url);
            return url.Substring(0, url.IndexOf("?", StringComparison.Ordinal) + 1) + $"{YoutubeVideoQueryParam}={videoId}";
        }

        private string GetJsUrl(string body)
        {
            var jsMatch = Regex.Match(body, "(\"js\":.*?.js)");
            if (!jsMatch.Success)
            {
                _logger.LogInformation($"{nameof(GetJsUrl)}: Js url was not found in json key = 'js', checking json key = 'jsUrl'...");
                jsMatch = Regex.Match(body, "(\"jsUrl\":.*?.js)");
            }

            if (!jsMatch.Success)
            {
                _logger.LogInformation($"{nameof(GetJsUrl)}: Js url was not found in player config, checking if we have a jsUrl key in the body...");
                jsMatch = Regex.Match(body, "(\"jsUrl\":.*?.js)");
            }

            string jsUrl = jsMatch.Value;
            if (!string.IsNullOrWhiteSpace(jsUrl))
            {
                string realOne = YoutubeUrl + jsUrl[jsUrl.IndexOf("/", StringComparison.Ordinal)..];
                return ReplaceWithAmpersand(realOne);
            }

            string msg = "Could not retrieve the js url";
            _logger.LogError($"{nameof(GetJsUrl)}: {msg}");
            throw new InvalidOperationException(msg);
        }

        private async Task GetAndSetRealUrl(
            YoutubeMedia media,
            string body,
            string jsUrl,
            string fmt,
            int desiredQuality,
            CancellationToken cancellationToken = default)
        {
            if (!body.Contains(YoutubePlayerConfig))
            {
                _logger.LogError($"{nameof(GetAndSetRealUrl)}: Url couldn't be parsed");
                throw new Exception("Url couldn't be parsed");
            }

            if (media.IsHls)
            {
                _logger.LogInformation($"{nameof(GetAndSetRealUrl)}: Url is an hls");
                return;
            }

            //Classic parameters - out of use since early 2020
            if (body.Contains(UrlEncodedStreamMap))
            {
                _logger.LogInformation($"{nameof(GetAndSetRealUrl)}: Body contains old parameters... Body = {body}");
                if (string.IsNullOrWhiteSpace(fmt))
                {
                    //TODO: NOT SURE ABOUT THIS REGEX
                    throw new NotImplementedException();
                }
                //TODO: COMPLETE THIS CASE
                int startIndex = body.IndexOf(UrlEncodedStreamMap, StringComparison.OrdinalIgnoreCase) + UrlEncodedStreamMap.Length + 1;  // is the opening "
                string urlMap = body.Substring(startIndex);
                int end = urlMap.IndexOf("\"", StringComparison.Ordinal);
                if (end > 0)
                {
                    urlMap = urlMap.Substring(0, end);
                }
                string url = GetUrlEncodedStream(urlMap);
                media.SetFromFormat(url);
                return;
            }

            VideoQualities qualities = media.VideoQualities;
            if (!qualities.UseAdaptiveFormats)
            {
                string stream = qualities.GetStreamFromFormats(desiredQuality);
                string url = await GetFinalUrl(stream, jsUrl, cancellationToken);
                media.SetFromFormat(url);
                return;
            }

            var (videoStream, audioStream) = qualities.GetStreamsFromAdaptiveFormats(desiredQuality);

            string finalVideoUrl = await GetFinalUrl(videoStream, jsUrl, cancellationToken);
            string finalAudioUrl = await GetFinalUrl(audioStream, jsUrl, cancellationToken);

            media.SetFromAdaptiveFormats(finalVideoUrl, finalAudioUrl);
        }

        private async Task<string> GetFinalUrl(
            string pickedQualityUrl,
            string jsUrl,
            CancellationToken cancellationToken = default)
        {
            string cipherPattern = @"(?<=\\""signatureCipher\\"":).+";
            string cipher = Regex.Match(pickedQualityUrl, cipherPattern).Value;
            if (string.IsNullOrEmpty(cipher))
            {
                _logger.LogInformation($"{nameof(GetAndSetRealUrl)}: SignatureUrl key was not found, checking for any other cipher...");
                cipher = Regex.Match(pickedQualityUrl, @"(?<=\""[a-zA-Z]*[Cc]ipher\"":).+").Value;
            }

            jsUrl = ReplaceWithAmpersand(jsUrl);
            string js = await GetContentFromUrl(jsUrl, cancellationToken);
            string finalUrl;
            if (string.IsNullOrEmpty(cipher))
            {
                _logger.LogInformation($"{nameof(GetAndSetRealUrl)}: Body doesn't contain a cipher, checking if the url is already unscrambled...");
                //Unscrambled signature, already included in ready-to-use URL
                string urlPattern = @"(?<=url\\"":\\"").*?(?=\\"")";
                if (!Regex.Match(pickedQualityUrl, urlPattern).Success)
                {
                    urlPattern = "(?<=url\":\").*?(?=\")";
                    _logger.LogInformation($"{nameof(GetAndSetRealUrl)}: Url not found, checking with another pattern...");
                }

                string url = Regex.Match(pickedQualityUrl, urlPattern).Value;
                finalUrl = DecodeUrlString(url);
            }
            else
            {
                _logger.LogInformation($"{nameof(GetAndSetRealUrl)}: Body contains a cipher...");
                //Scrambled signature: some assembly required
                finalUrl = GetUrlFromCipher(cipher, js);
            }

            if (string.IsNullOrWhiteSpace(finalUrl))
            {
                _logger.LogWarning($"{nameof(GetAndSetRealUrl)}: Couldn't retrieve the final url");
                throw new UrlCouldNotBeParsedException("Couldn't retrieve the final url");
            }

            //The "n" parameter is scrambled too, and needs to be descrambled
            //and replaced in place, otherwise the data transfer gets throttled
            //down to between 40 and 80 kB / s, below real-time playability level.
            _logger.LogInformation($"{nameof(GetAndSetRealUrl)}: Checking if the n parameter is present...");
            string nPattern = "[?&]n=([^&]+)";
            string nMatch = Regex.Match(finalUrl, nPattern).Value;
            if (string.IsNullOrWhiteSpace(nMatch))
            {
                _logger.LogInformation($"{nameof(GetAndSetRealUrl)}: No n parameter was found");
                return finalUrl;
            }

            _logger.LogInformation($"{nameof(GetAndSetRealUrl)}: N parameter was found, descrambling it...");
            //the substring is to remove the &n=
            nMatch = DecodeUrlString(nMatch)[3..];
            string descrambledN = _nDescrambler.NDescramble(nMatch, js);
            if (string.IsNullOrWhiteSpace(descrambledN))
            {
                return finalUrl;
            }
            return Regex.Replace(finalUrl, nPattern, $"&n={descrambledN}");
        }

        private async Task<string> GetContentFromUrl(
            string url,
            CancellationToken cancellationToken = default)
        {
            int attempts = 1;
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.45 Safari/537.36");
            while (attempts >= 0)
            {
                attempts--;
                try
                {
                    var response = await httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
                    if (!response.IsSuccessStatusCode && attempts < 0)
                    {
                        break;
                    }

                    return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    _logger.LogError(e,
                        $"{nameof(GetContentFromUrl)}: Unknown error while trying to retrieve content " +
                        $"from url = {url}. Attempts = {attempts}");
                }
            }

            _logger.LogWarning($"{nameof(GetContentFromUrl)}: Couldn't retrieve content from url = {url}.");
            throw new UrlCouldNotBeParsedException(url);
        }

        private List<VideoQuality> GetVideoQualitiesFromFormats(string body)
        {
            _logger.LogInformation($"{nameof(GetVideoQualities)}: Getting video qualities...");
            string formatPattern = @"(\\""formats\\"":\[.*?])";
            var formatMatch = Regex.Match(body, formatPattern);

            if (formatMatch.Length == 0)
            {
                _logger.LogInformation(
                    $"{nameof(GetVideoQualities)}: Couldn't retrieve formats, " +
                    "checking if the formats is not between slashes using the format keyword...");
                formatPattern = @"(\""formats\"":\[.*?])";
                formatMatch = Regex.Match(body, formatPattern);
            }

            return string.IsNullOrWhiteSpace(formatMatch.Value)
                ? new List<VideoQuality>()
                : ProcessObtainedFormats(formatMatch.Value, false);
        }

        private List<VideoQuality> GetVideoQualitiesFromAdaptiveFormats(string body)
        {
            //https://javascript.plainenglish.io/make-your-own-youtube-downloader-626133572429
            _logger.LogInformation($"{nameof(GetVideoQualities)}: Getting video qualities...");
            string formatPattern = @"(\\""adaptiveFormats\\"":\[.*?])";
            var formatMatch = Regex.Match(body, formatPattern);

            if (formatMatch.Length == 0)
            {
                _logger.LogInformation(
                    $"{nameof(GetVideoQualities)}: Couldn't retrieve adaptive formats, " +
                    "checking if the formats is not between slashes using the format keyword...");
                formatPattern = @"(\""adaptiveFormats\"":\[.*?])";
                formatMatch = Regex.Match(body, formatPattern);
            }

            return string.IsNullOrWhiteSpace(formatMatch.Value)
                ? new List<VideoQuality>()
                : ProcessObtainedFormats(formatMatch.Value, true);
        }

        private VideoQualities GetVideoQualities(string body)
        {
            _logger.LogInformation($"{nameof(GetVideoQualities)}: Getting video qualities...");
            List<VideoQuality> fromFormats = GetVideoQualitiesFromFormats(body);
            List<VideoQuality> fromAdaptiveFormats = GetVideoQualitiesFromAdaptiveFormats(body);

            if (fromFormats.Count != 0 || fromAdaptiveFormats.Count != 0)
                return new VideoQualities(fromFormats, fromAdaptiveFormats);

            _logger.LogWarning($"{nameof(Parse)}: Couldn't retrieve any qualities from body");
            throw new UrlCouldNotBeParsedException("Couldn't retrieve any qualities");
        }

        private List<VideoQuality> ProcessObtainedFormats(string formatMatch, bool isAdaptive)
        {
            string streamMap = ReplaceWithAmpersand(DecodeUrlString(formatMatch));

            string heightPatternA = @"(?<=\\""height\\"":).+?(?=,)";
            string heightPatternB = @"(?<=\""height\"":).+?(?=,)";
            string videoPattern = "\"mimeType\":\"video.mp4";
            string audioPattern = "\"mimeType\":\"audio.mp4";
            //https://stackoverflow.com/questions/14952113/how-can-i-match-nested-brackets-using-regex
            var streams = PcreRegex.Matches(streamMap, @"\{((?>[^{}]+)|(?R))*\}")
                .Where(match => !isAdaptive ||
                                Regex.IsMatch(match.Value, videoPattern) ||
                                Regex.IsMatch(match.Value, audioPattern))
                .Select(stream => stream.Value)
                .ToList();
            var qualities = new List<VideoQuality>();
            foreach (string stream in streams)
            {
                if (Regex.IsMatch(stream, audioPattern))
                {
                    qualities.Add(VideoQuality.OnlyAudio(stream));
                    continue;
                }

                var val = Regex.Match(stream, heightPatternA).Value;
                if (string.IsNullOrEmpty(val))
                {
                    val = Regex.Match(stream, heightPatternB).Value;
                }

                if (string.IsNullOrEmpty(val))
                {
                    _logger.LogWarning($"{nameof(GetVideoQualities)}: Couldn't retrieve height from = {stream}");
                }

                int quality = int.Parse(string.IsNullOrEmpty(val) ? "-1" : val);
                if (qualities.All(q => q.Quality != quality) && quality > 0)
                {
                    qualities.Add(isAdaptive
                        ? VideoQuality.OnlyVideo(stream, quality)
                        : VideoQuality.VideoAndAudio(stream, quality));
                }
            }

            _logger.LogInformation($"{nameof(GetVideoQualities)}: Got = {qualities.Count} qualities");
            return qualities;
        }

        private string GetUrlEncodedStream(string stream)
        {
            string str = ReplaceWithAmpersand(DecodeUrlString(stream));
            string urlMap = str.Substring(str.IndexOf("url=http", StringComparison.OrdinalIgnoreCase) + 4);
            // search urlMap until we see either a & or ,
            var sb = new StringBuilder();
            foreach (var t in urlMap)
            {
                if (t == '&' || t == ',')
                    break;
                sb.Append(t);
            }
            return sb.ToString();
        }

        private string DecodeUrlString(string url)
        {
            string newUrl;
            while ((newUrl = Uri.UnescapeDataString(url)) != url)
                url = newUrl;
            return newUrl;
        }

        // replace all the \u0026 with &
        private string ReplaceWithAmpersand(string val)
            => val.Replace("\\u0026", "&").Replace("\u0026", "&");

        private string GetQueryParam(string url, string name)
        {
            var queryParams = GetQueryParams(url);
            return queryParams.Get(name);
        }

        private NameValueCollection GetQueryParams(string url)
        {
            var uri = new Uri(url);

            // you can check host here => uri.Host <= "www.youtube.com"
            return HttpUtility.ParseQueryString(uri.Query);
        }

        private string GetKeyContentValue(string str)
        {
            if (string.IsNullOrEmpty(str))
                return null;
            var contentStr = new StringBuilder();
            int contentStart = str.IndexOf(ContentValueKeyWord, StringComparison.OrdinalIgnoreCase) + ContentValueKeyWord.Length;
            if (contentStart <= 0)
                return contentStr.ToString();
            while (true)
            {
                var ch = str[contentStart++];
                if (ch == '\"')
                    break;
                contentStr.Append(ch);
            }
            return contentStr.ToString();
        }

        private string GetVideoKeywordValue(string body, string keyword)
        {
            if (!body.Contains(keyword))
                return null;
            int index = body.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
            var value = new StringBuilder();
            char ch;
            do
            {
                ch = body[index++];
                value.Append(ch);
            }
            while (ch != '>');
            return GetKeyContentValue(value.ToString());
        }

        private string GetUrlFromCipher(string cipher, string js)
        {
            cipher = ReplaceWithAmpersand(cipher);

            string urlPattern = @"(?<=url[^&]+).*?(?=\\"")";
            var urlMatch = Regex.Match(cipher, urlPattern);
            if (!urlMatch.Success)
            {
                _logger.LogInformation($"{nameof(GetUrlFromCipher)}: No match was found, trying without the backslash in the pattern...");
                urlMatch = Regex.Match(cipher, @"(?<=url[^&]+).*?(?=\"")");
            }

            if (string.IsNullOrEmpty(urlMatch.Value))
            {
                var msg = $"Couldn't retrieve url from cipher = {cipher}";
                _logger.LogWarning($"{nameof(GetUrlFromCipher)}: {msg}");
                throw new Exception(msg);
            }

            string url = DecodeUrlString(urlMatch.Value.Replace("\\/", "/"));

            //Descramble any scrambled signature and append it to URL
            string sPattern = @"(?<=\\""s=)([^&]+)";
            string s = DecodeUrlString(Regex.Match(cipher, sPattern).Value);
            if (string.IsNullOrEmpty(s))
            {
                _logger.LogInformation($"{nameof(GetUrlFromCipher)}: Couldn't find the sPattern in cipher, trying without the backslash in the pattern...");
                s = DecodeUrlString(Regex.Match(cipher, @"(?<=\""s=)([^&]+)").Value);
            }

            if (string.IsNullOrEmpty(s))
            {
                _logger.LogWarning($"{nameof(GetUrlFromCipher)}: Couldn't find the sPattern in cipher = {cipher}");
                return url;
            }

            s = _jsDescrambler.JsDescramble(s, js);

            string sp = Regex.Match(cipher, "sp=([^&]+)").Value.Split("=".ToCharArray()).Last();
            if (string.IsNullOrEmpty(sp))
            {
                sp = "signature";
            }

            return $"{url}&{sp}={Uri.EscapeDataString(s)}";
        }
    }
}
