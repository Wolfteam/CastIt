using CastIt.Application.Common.Extensions;
using CastIt.Application.Interfaces;
using CastIt.Domain.Models.Youtube;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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

namespace CastIt.Application.Youtube
{
    public class YoutubeUrlDecoder : IYoutubeUrlDecoder
    {
        private const string UrlEncodedStreamMap = "\"url_encoded_fmt_stream_map\":";
        private const string TitleKeyWord = "<meta property=\"og:title\"";
        private const string DescriptionKeyWord = "<meta name=\"description\"";
        private const string ThumbnailKeyWord = "<meta property=\"og:image\"";
        private const string ContentValueKeyWord = "content=\"";
        private const string YoutubeUrl = "https://www.youtube.com";
        private const string YoutubePlayerConfig = "ytplayer.config";

        private const string YouTubePlayListPath = "/playlist";
        private const string YoutubePlayListQueryParam = "list";
        private const string YoutubeVideoQueryParam = "v";

        private const int DefaultQuality = 360;

        private readonly ILogger<YoutubeUrlDecoder> _logger;

        public YoutubeUrlDecoder(ILogger<YoutubeUrlDecoder> logger)
        {
            _logger = logger;
        }

        public bool IsYoutubeUrl(string url)
        {
            return url.StartsWith(YoutubeUrl, StringComparison.OrdinalIgnoreCase);
        }

        public bool IsPlayListAndVideo(string url)
        {
            var query = GetQueryParams(url);
            return IsPlayList(url) && query.AllKeys.Contains(YoutubeVideoQueryParam, StringComparer.OrdinalIgnoreCase);
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

        public async Task<YoutubeMedia> Parse(
            string url,
            int? desiredQuality = null,
            bool getFinalUrl = true)
        {
            desiredQuality ??= DefaultQuality;
            _logger.LogInformation($"{nameof(Parse)}: Trying to parse url = {url}");
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Couldn't retrieve youtube video. StatusCode = {response.StatusCode}");

            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            body = body.Replace("\\/", "/");

            var (isHls, _) = IsHls(body);
            var media = new YoutubeMedia
            {
                IsHls = isHls,
                Title = GetVideoKeywordValue(body, TitleKeyWord),
                Description = GetVideoKeywordValue(body, DescriptionKeyWord),
                ThumbnailUrl = GetVideoKeywordValue(body, ThumbnailKeyWord)
            };

            if (getFinalUrl)
            {
                var qualities = GetVideoQualities(body);
                if (!qualities.Any())
                {
                    _logger.LogWarning($"{nameof(Parse)}: Couldn't retrieve any qualities for video = {url} in body = {body}");
                    throw new InvalidOperationException($"Couldn't retrieve any qualities for video = {url}");
                }
                media.Qualities.AddRange(qualities.Select(q => q.Key));
                int closest = qualities
                    .Select(k => k.Key)
                    .GetClosest(desiredQuality.Value);

                media.SelectedQuality = closest;
                _logger.LogInformation($"{nameof(Parse)}: Selected quality = {closest}");

                string pick = qualities.First(kvp => kvp.Key == closest).Value;
                media.Url = await GetFinalUrl(body, pick);
            }
            else
            {
                return media;
            }

            if (string.IsNullOrEmpty(media.Url))
            {
                _logger.LogInformation($"{nameof(Parse)}: Url couldn't be parsed");
                throw new Exception($"Url couldn't be parsed for url = {url}");
            }

            _logger.LogInformation($"{nameof(Parse)}: Url was completely parsed. Media = {JsonConvert.SerializeObject(media)}");

            return media;
        }

        public async Task<List<string>> ParseYouTubePlayList(string url, CancellationToken token)
        {
            var links = new List<string>();
            _logger.LogInformation($"{nameof(ParseYouTubePlayList)}: Parsing url = {url}");

            //Here we got an url like this: https://www.youtube.com/watch?v=somevideoid
            if (IsPlayListAndVideo(url))
            {
                //Here we want an url like this: https://www.youtube.com/playlist?list=someplaylistid
                string playlistId = GetPlayListId(url);
                url = $"{YoutubeUrl}{YouTubePlayListPath}?{YoutubePlayListQueryParam}={playlistId}";
                _logger.LogInformation($"{nameof(ParseYouTubePlayList)}: Url is playlist and video, the final url will be = {url}");
            }

            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url, token).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"{nameof(ParseYouTubePlayList)}: Response is not success status code. Code = {response.StatusCode}");
                return links;
            }

            var body = await response.Content.ReadAsStringAsync(token).ConfigureAwait(false);
            var html = new HtmlDocument();
            html.LoadHtml(body);
            var root = html.DocumentNode;
            var table = root.Descendants().FirstOrDefault(f => f.Id == "pl-video-table");
            //Sometimes the html returns a json, other times it returns a table
            if (table != null)
            {
                _logger.LogInformation($"{nameof(ParseYouTubePlayList)}: Body contains table, parsing it...");
                links = table.Descendants("a")
                    .Where(node => node.HasClass("pl-video-title-link"))
                    .Select(node => RemoveNotNeededParams(YoutubeUrl + node.GetAttributeValue("href", string.Empty)))
                    .Where(link => link.StartsWith(YoutubeUrl, StringComparison.OrdinalIgnoreCase))
                    .Distinct()
                    .ToList();
            }
            else
            {
                _logger.LogInformation($"{nameof(ParseYouTubePlayList)}: Body contains pure javascript, parsing it...");
                string pattern = @"(\/watch).*?(?="")";
                body = ReplaceWithAmpersand(body).Replace("\\/", "/");
                links = Regex.Matches(body, pattern)
                    .Select(match => RemoveNotNeededParams(YoutubeUrl + match.Value))
                    .Where(link => link.StartsWith(YoutubeUrl, StringComparison.OrdinalIgnoreCase))
                    .Distinct()
                    .ToList();
            }
            _logger.LogInformation($"{nameof(ParseYouTubePlayList)}: Got {links.Count} link(s)");

            return links;
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

        private string GetURLEncodedStream(string stream)
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

        private async Task<string> GetUrlFromCipher(string cipher, string jsUrl)
        {
            jsUrl = ReplaceWithAmpersand(jsUrl);
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

            s = await JsDescramble(s, jsUrl);

            string sp = Regex.Match(cipher, "sp=([^&]+)").Value.Split("=".ToCharArray()).Last();
            if (string.IsNullOrEmpty(sp))
            {
                sp = "signature";
            }

            return $"{url}&{sp}={Uri.EscapeDataString(s)}";
        }

        private async Task<string> JsDescramble(string s, string jsUrl)
        {
            //Fetch javascript code
            _logger.LogInformation($"{nameof(JsDescramble)}: Fetching js code from jsUrl = {jsUrl}");
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(jsUrl).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"{nameof(JsDescramble)}: Status code does not indicate success. = {response.StatusCode}");
                return s;
            }

            string js = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            //Look for the descrambler function's name (in this example its "pt")
            //if (k.s) { var l=k.sp, m=pt(decodeURIComponent(k.s)); f.set(l, encodeURIComponent(m))}
            //k.s(from stream map field "s") holds the input scrambled signature
            //k.sp(from stream map field "sp") holds a parameter name(normally
            //"signature" or "sig") to set with the output, descrambled signature
            string descramblerPattern = @"(?<=[,&|]).(=).+(?=\(decodeURIComponent)";
            var descramblerMatch = Regex.Match(js, descramblerPattern);
            if (string.IsNullOrEmpty(descramblerMatch.Value))
            {
                _logger.LogInformation($"{nameof(JsDescramble)}: Coudln't retrieve the descrambler function");
                return s;
            }

            string descrambler = descramblerMatch.Value.Substring(descramblerMatch.Value.IndexOf("=", StringComparison.Ordinal) + 1);
            if (SpecialCharsExists(descrambler))
            {
                _logger.LogInformation($"{nameof(JsDescramble)}: Descrambler = {descrambler} contains special chars, escaping the first one");
                descrambler = @$"\{descrambler}";
            }

            //Fetch the code of the descrambler function
            //Go = function(a){ a = a.split(""); Fo.sH(a, 2); Fo.TU(a, 28); Fo.TU(a, 44); Fo.TU(a, 26); Fo.TU(a, 40); Fo.TU(a, 64); Fo.TR(a, 26); Fo.sH(a, 1); return a.join("")};
            string rulesPattern = $"{descrambler}=function.+(?=;)";
            _logger.LogInformation($"{nameof(JsDescramble)}: Getting the rules by using the following pattern = {rulesPattern}");

            string rules = Regex.Match(js, rulesPattern).Value;
            if (string.IsNullOrEmpty(rules))
            {
                _logger.LogInformation($"{nameof(JsDescramble)}: Couldn't retrieve the rules function");
                return s;
            }

            //Get the name of the helper object providing transformation definitions
            string helperPattern = @"(?<=;).*?(?=\.)";
            var helperMatch = Regex.Match(rules, helperPattern);
            if (string.IsNullOrEmpty(helperMatch.Value))
            {
                _logger.LogInformation($"{nameof(JsDescramble)}: Couldn't retrieve signature transformation helper name");
                return s;
            }

            string helper = Regex.Split(helperMatch.Value, @"\W+")
                .GroupBy(g => g)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(helper))
            {
                _logger.LogInformation($"{nameof(JsDescramble)}: Couldn't retrieve signature transformation helper name from = {helperMatch.Value}");
                return s;
            }

            //Fetch the helper object code
            //var Fo ={ TR: function(a){ a.reverse()},TU: function(a, b){ var c = a[0]; a[0] = a[b % a.length]; a[b] = c},sH: function(a, b){ a.splice(0, b)} };
            string transformationsPattern = "var " + helper + @"=(.*?)+\n.+\n.+(?=};)";
            string transformations = Regex.Match(js, transformationsPattern).Value;

            //Parse the helper object to map available transformations
            var methods = Regex.Matches(transformations, "(..):.*?(?=})");
            var trans = new Dictionary<string, string>();

            foreach (var method in methods)
            {
                string m = method.ToString();
                string methodName = m.Split(":".ToCharArray())[0];
                if (m.Contains(".reverse("))
                {
                    trans.Add(methodName, "reverse");
                }
                else if (m.Contains(".splice("))
                {
                    trans.Add(methodName, "slice");
                }
                else if (m.Contains("var c="))
                {
                    trans.Add(methodName, "swap");
                }
            }

            //Parse descrambling rules, map them to known transformations
            //and apply them on the signature
            var rulesToApply = Regex.Matches(rules, helper + @"\..*?(?=;)");
            var commaSeparator = new[] { ',' };
            foreach (var rule in rulesToApply)
            {
                //zw.sH(a,8)
                string x = rule.ToString();

                //sH(a, 2)
                string transToApply = x.Split(".".ToCharArray()).Last();

                //sH
                string transName = transToApply.Substring(0, transToApply.IndexOf("(", StringComparison.Ordinal));
                switch (trans[transName])
                {
                    case "reverse":
                        s = new string(s.Reverse().ToArray());
                        break;
                    case "slice":
                        {
                            int value = int.Parse(transToApply.Split(commaSeparator).Last().Replace(")", ""));
                            s = s.Substring(value);
                            break;
                        }
                    case "swap":
                        {
                            int value = int.Parse(transToApply.Split(commaSeparator).Last().Replace(")", ""));
                            var c = s[0];
                            s = s.ReplaceAt(0, s[value % s.Length]);
                            s = s.ReplaceAt(value % s.Length, c);
                            break;
                        }
                }
            }

            return s;
        }

        private string DecodeUrlString(string url)
        {
            string newUrl;
            while ((newUrl = Uri.UnescapeDataString(url)) != url)
                url = newUrl;
            return newUrl;
        }

        private string RemoveNotNeededParams(string url)
        {
            string videoId = GetVideoId(url);
            return url.Substring(0, url.IndexOf("?", StringComparison.Ordinal) + 1) + $"{YoutubeVideoQueryParam}={videoId}";
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

        private NameValueCollection GetQueryParams(string url)
        {
            var uri = new Uri(url);

            // you can check host here => uri.Host <= "www.youtube.com"
            return HttpUtility.ParseQueryString(uri.Query);
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

        private Dictionary<int, string> GetVideoQualities(string body)
        {
            _logger.LogInformation($"{nameof(GetVideoQualities)}: Getting video qualities...");
            var formatPattern = @"(\\""formats\\"":\[.*?])";
            var formatMatch = Regex.Match(body, formatPattern);
            if (formatMatch.Length == 0)
            {
                _logger.LogInformation($"{nameof(GetVideoQualities)}: Couldn't retrieve formats, checking if we have adaptiveFormats...");
                formatPattern = @"(\\""adaptiveFormats\\"":\[.*?])";
                formatMatch = Regex.Match(body, formatPattern);
            }

            if (formatMatch.Length == 0)
            {
                _logger.LogInformation(
                    $"{nameof(GetVideoQualities)}: Couldn't retrieve formats, " +
                    "checking if the formats is not between slashes using the format keyword...");
                formatPattern = @"(\""formats\"":\[.*?])";
                formatMatch = Regex.Match(body, formatPattern);
            }

            if (formatMatch.Length == 0)
            {
                _logger.LogInformation(
                    $"{nameof(GetVideoQualities)}: Couldn't retrieve formats, " +
                    "checking if the formats is not between slashes using the adaptiveFormats keyword...");
                formatPattern = @"(\""adaptiveFormats\"":\[.*?])";
                formatMatch = Regex.Match(body, formatPattern);
            }

            if (formatMatch.Length == 0)
            {
                _logger.LogWarning($"{nameof(GetVideoQualities)}: Couldn't retrieve qualities for body = {body}...");
                throw new Exception("Couldn't retrieve video qualities");
            }

            string streamMap = ReplaceWithAmpersand(DecodeUrlString(formatMatch.Value));

            string heightPatternA = @"(?<=\\""height\\"":).+?(?=,)";
            string heightPatternB = @"(?<=\""height\"":).+?(?=,)";
            var streams = Regex.Matches(streamMap, "{(.*?)}").AsQueryable().OfType<Match>().ToList();
            var qualities = streams.ToDictionary(k =>
            {
                var val = Regex.Match(k.ToString(), heightPatternA).Value;
                if (string.IsNullOrEmpty(val))
                {
                    _logger.LogInformation($"Couldn't retrieve height, trying now without backslashes in the pattern");
                    val = Regex.Match(k.ToString(), heightPatternB).Value;
                }

                if (string.IsNullOrEmpty(val))
                {
                    _logger.LogWarning($"Couldn't retrieve height from = {k}");
                }

                return int.Parse(string.IsNullOrEmpty(val) ? "-1" : val);
            }, v => v.ToString());

            var qualitiesDictionary = qualities.Where(kvp => kvp.Key > 0).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            _logger.LogInformation($"{nameof(GetVideoQualities)}: Got = {qualitiesDictionary.Count} video qualities");

            return qualitiesDictionary;
        }

        private async Task<string> GetFinalUrl(string body, string pickedQualityUrl)
        {
            if (body.Contains(YoutubePlayerConfig))
            {
                _logger.LogInformation($"{nameof(GetFinalUrl)}: Body contains yt player config js...");
                //body = body.Replace("\\/", "/");
                var (isHls, liveUrl) = IsHls(body);
                if (isHls)
                {
                    _logger.LogInformation($"{nameof(GetFinalUrl)}: Url is an hls");
                    //media.Url = liveMatch.Value;
                    //media.IsHls = true;
                    return liveUrl;
                }

                int start = body.IndexOf(YoutubePlayerConfig, StringComparison.OrdinalIgnoreCase);
                string playerConfig = body.Substring(start);
                playerConfig = playerConfig
                    .Substring(0, playerConfig.IndexOf("</script>", StringComparison.OrdinalIgnoreCase))
                    .Replace("\\/", "/");
                var jsMatch = Regex.Match(playerConfig, "(\"js\":.*?.js)");
                if (!jsMatch.Success)
                {
                    _logger.LogInformation($"{nameof(GetFinalUrl)}: Js url was not found in json key = 'js', checking json key = 'jsUrl'...");
                    jsMatch = Regex.Match(playerConfig, "(\"jsUrl\":.*?.js)");
                }

                if (!jsMatch.Success)
                {
                    _logger.LogInformation($"{nameof(GetFinalUrl)}: Js url was not found in player config, checking if we have a jsUrl key in the body...");
                    jsMatch = Regex.Match(body, "(\"jsUrl\":.*?.js)");
                }

                string jsUrl = jsMatch.Value;
                if (string.IsNullOrWhiteSpace(jsUrl))
                {
                    var msg = "Could not retrieve the js url";
                    _logger.LogError($"{nameof(GetFinalUrl)}: {msg}");
                    throw new InvalidOperationException(msg);
                }

                jsUrl = YoutubeUrl + jsUrl.Substring(jsUrl.IndexOf("/", StringComparison.Ordinal));

                if (body.Contains(UrlEncodedStreamMap))
                {
                    _logger.LogInformation($"{nameof(GetFinalUrl)}: Body contains old parameters... Body = {body}");
                    //TODO: COMPLETE THIS CASE
                    int startIndex = body.IndexOf(UrlEncodedStreamMap, StringComparison.OrdinalIgnoreCase) + UrlEncodedStreamMap.Length + 1;  // is the opening "
                    string urlMap = body.Substring(startIndex);
                    int end = urlMap.IndexOf("\"", StringComparison.Ordinal);
                    if (end > 0)
                    {
                        urlMap = urlMap.Substring(0, end);
                    }
                    return GetURLEncodedStream(urlMap);
                }

                string cipherPattern = @"(?<=\\""signatureCipher\\"":).+";
                string cipher = Regex.Match(pickedQualityUrl, cipherPattern).Value;
                if (string.IsNullOrEmpty(cipher))
                {
                    _logger.LogInformation($"{nameof(GetFinalUrl)}: SignatureUrl key was not found, checking for any other cipher...");
                    cipher = Regex.Match(pickedQualityUrl, @"(?<=\""[a-zA-Z]*[Cc]ipher\"":).+").Value;
                }

                if (string.IsNullOrEmpty(cipher))
                {
                    _logger.LogInformation($"{nameof(GetFinalUrl)}: Body doesn't contain a cipher...");
                    //Unscrambled signature, already included in ready-to-use URL
                    string urlPattern = @"(?<=url\\"":\\"").*?(?=\\"")";
                    return DecodeUrlString(Regex.Match(pickedQualityUrl, urlPattern).Value);
                }

                _logger.LogInformation($"{nameof(GetFinalUrl)}: Body contains a cipher...");
                //Scrambled signature: some assembly required
                return await GetUrlFromCipher(cipher, jsUrl).ConfigureAwait(false);
            }

            _logger.LogError($"{nameof(GetFinalUrl)}: Url couldn't be parsed");
            throw new Exception("Url couldn't be parsed");
        }

        private (bool, string) IsHls(string body)
        {
            string livePattern = @"(?<=hlsManifestUrl\\"":\\"").*?(?=\\)";
            var liveMatch = Regex.Match(body, livePattern);
            return (liveMatch.Success, liveMatch.Value);
        }

        private bool SpecialCharsExists(string input)
        {
            char[] one = input.ToCharArray();
            char[] two = new char[one.Length];
            int c = 0;
            foreach (var t in one)
            {
                if (char.IsLetterOrDigit(t))
                    continue;
                two[c] = t;
                c++;
            }

            Array.Resize(ref two, c);
            return two.Length > 0;
        }

        // replace all the \u0026 with &
        private string ReplaceWithAmpersand(string val)
            => val.Replace("\\u0026", "&").Replace("\u0026", "&");
    }
}
