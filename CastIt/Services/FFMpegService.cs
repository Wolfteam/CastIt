using CastIt.Common.Utils;
using CastIt.Interfaces;
using CastIt.Models.FFMpeg;
using EmbedIO;
using MvvmCross.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CastIt.Services
{
    public class FFMpegService : IFFMpegService
    {
        private readonly IMvxLog _logger;

        private readonly Process _generateThumbnailProcess;
        private readonly Process _generateAllThumbnailsProcess;
        private readonly Process _transcodeProcess;

        private static readonly IReadOnlyList<string> AllowedVideoContainers = new List<string>
        {
            ".mp4"
        };
        private static readonly IReadOnlyList<string> AllowedVideoCodecs = new List<string>
        {
            "h264"
        };
        private static readonly IReadOnlyList<string> AllowedVideoProfiles = new List<string>
        {
            "Main", "High"
        };
        private const int MaxVideoLevel = 41;

        private static readonly IReadOnlyList<string> AllowedMusicContainers = new List<string>
        {
            ".aac", ".mp3"
        };
        private static readonly IReadOnlyList<string> AllowedMusicCodecs = new List<string>
        {
            "mp3", "aac"
        };

        private static readonly IReadOnlyList<string> AllowedMusicProfiles = new List<string>
        {
            "HE-AAC", "LC-AAC", "LC", "HE"
        };

        private bool _checkGenerateThumbnailProcess;
        private bool _checkGenerateAllThumbnailsProcess;

        public FFMpegService(IMvxLogProvider logProvider)
        {
            _logger = logProvider.GetLogFor<FFMpegService>();
            _transcodeProcess = new Process
            {
                EnableRaisingEvents = true,
                StartInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = FileUtils.GetFFMpegPath(),
                    UseShellExecute = false,
                    LoadUserProfile = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            var startInfo = new ProcessStartInfo
            {
                FileName = FileUtils.GetFFMpegPath(),
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            _generateThumbnailProcess = new Process
            {
                StartInfo = startInfo
            };

            _generateAllThumbnailsProcess = new Process
            {
                StartInfo = startInfo
            };
        }

        public string GetThumbnail(string mrl, int second)
        {
            if (!FileUtils.IsLocalFile(mrl))
            {
                _logger.Warn($"{nameof(GetThumbnail)}: Cant get thumbnail for file = {mrl}. Its not a local file");
                return null;
            }

            var filename = Path.GetFileName(mrl);
            var ext = Path.GetExtension(mrl);
            var thumbnailPath = FileUtils.GetThumbnailFilePath(filename, second);
            if (File.Exists(thumbnailPath))
            {
                return thumbnailPath;
            }

            //-i filename(the source video's file path)
            //- deinterlace(converts interlaced video to non - interlaced form)
            //- an(audio recording is disabled; we don't need it for thumbnails)
            //- ss position(input video is decoded and dumped until the timestamp reaches position, 
            //     our thumbnail's position in seconds)
            //- t duration(the output file stops writing after duration seconds)
            //-y(overwrites the output file if it already exists)
            //-s widthxheight(size of the frame in pixels)
            //https://stackoverflow.com/questions/48425905/cmd-exe-via-system-diagnostics-process-cannot-parse-an-argument-with-spaces
            string cmd;

            if (FileUtils.IsMusicFile(mrl))
            {
                cmd = $@"-y -i ""{mrl}"" -an -filter:v scale=200:150 ""{thumbnailPath}""";
            }
            else
            {
                cmd = $@"-ss {second} -an -y -i ""{mrl}"" -vframes 1 -s 200x150 ""{thumbnailPath}""";
            }
            try
            {
                _logger.Info($"{nameof(GetThumbnail)}: Generating first thumbnail for file = {mrl}. Cmd = {cmd}");
                _checkGenerateThumbnailProcess = true;
                _generateThumbnailProcess.StartInfo.Arguments = cmd;
                _generateThumbnailProcess.Start();
                _generateThumbnailProcess.WaitForExit();
                _logger.Info($"{nameof(GetThumbnail)}: First thumbnail was succesfully generated for file = {mrl}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"{nameof(GenerateThumbmnails)}: Unknown error occurred");
            }
            finally
            {
                _checkGenerateThumbnailProcess = false;
            }
            return thumbnailPath;
        }

        public void GenerateThumbmnails(string mrl)
        {
            if (!FileUtils.IsLocalFile(mrl))
            {
                return;
            }
            var filename = Path.GetFileName(mrl);
            var ext = Path.GetExtension(mrl);

            if (FileUtils.IsMusicFile(mrl))
            {
                _logger.Info($"{nameof(GenerateThumbmnails)}: File = {mrl} is a music one, so we wont generate thumbnails for it");
                return;
            }
            var thumbnailPath = FileUtils.GetPreviewThumbnailFilePath(filename);
            var cmd = $@" -y -i ""{mrl}"" -an -vf fps=1/5 -s 200x150 ""{thumbnailPath}""";
            try
            {
                _logger.Info($"{nameof(GenerateThumbmnails)}: Generating all thumbnails for file = {mrl}. Cmd = {cmd}");
                _checkGenerateAllThumbnailsProcess = true;
                _generateThumbnailProcess.StartInfo.Arguments = cmd;
                _generateAllThumbnailsProcess.Start();
                _generateAllThumbnailsProcess.WaitForExit();
                _logger.Info($"{nameof(GenerateThumbmnails)}: All thumbnails were succesfully generated for file = {mrl}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"{nameof(GenerateThumbmnails)}: Unknown error occurred");
            }
            finally
            {
                _checkGenerateAllThumbnailsProcess = false;
            }
        }

        public void KillThumbnailProcess()
        {
            _logger.Info($"{nameof(KillThumbnailProcess)}: Killing thumbnail process");
            try
            {
                if (_checkGenerateAllThumbnailsProcess &&
                    !_generateAllThumbnailsProcess.HasExited)
                {
                    _logger.Info($"{nameof(KillThumbnailProcess)}: Killing the generate all thumbnails process");
                    _generateAllThumbnailsProcess.Kill(true);
                    _generateAllThumbnailsProcess.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                if (ex is InvalidOperationException)
                    return;
                _logger.Error(ex, $"{nameof(KillThumbnailProcess)}: Could not stop the generate all thumbnails process");
            }

            try
            {
                if (_checkGenerateThumbnailProcess &&
                    !_generateThumbnailProcess.HasExited)
                {
                    _logger.Info($"{nameof(KillThumbnailProcess)}: Killing the generate thumbnail process");
                    _generateThumbnailProcess.Kill(true);
                    _generateThumbnailProcess.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                if (ex is InvalidOperationException)
                    return;
                _logger.Error(ex, $"{nameof(KillThumbnailProcess)}: Could not stop the generate thumbnail process");
            }
        }

        public Task<double> GetFileDuration(string mrl, CancellationToken token)
        {
            bool isLocal = FileUtils.IsLocalFile(mrl);
            bool isUrl = FileUtils.IsUrlFile(mrl);
            if (!isLocal || isUrl)
            {
                _logger.Info($"{nameof(GetFileDuration)}: Couldnt retrieve duration for file = {mrl}. It is not a local file");
                return Task.FromResult<double>(-1);
            }

            try
            {
                return Task.Run(() =>
                {
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            WindowStyle = ProcessWindowStyle.Hidden,
                            FileName = FileUtils.GetFFprobePath(),
                            UseShellExecute = false,
                            LoadUserProfile = false,
                            RedirectStandardInput = true,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true
                        }
                    };
                    process.StartInfo.Arguments = @$"-v quiet -i ""{mrl}"" -show_entries format=duration -of csv=p=0";
                    process.Start();
                    process.WaitForExit();
                    string stringDuration = process.StandardOutput.ReadToEnd();
                    return double.Parse(stringDuration.Replace(Environment.NewLine, string.Empty));
                }, token);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"{nameof(GetFileDuration)}: Unknown error occurred");
                return Task.FromResult<double>(0);
            }
        }

        public void KillTranscodeProcess()
        {
            _logger.Info($"{nameof(KillTranscodeProcess)}: Killing transcode process");
            try
            {
                if (!_transcodeProcess.HasExited)
                {
                    _transcodeProcess.Kill(true);
                    _transcodeProcess.WaitForExit();
                }
            }
            catch (Exception e)
            {
                if (e is InvalidOperationException)
                    return;
                _logger.Error(e, $"{nameof(KillTranscodeProcess)}: Unknown error occurred");
            }
        }

        public Task<FFProbeFileInfo> GetFileInfo(string filePath, CancellationToken token)
        {
            _logger.Trace($"{nameof(GetFileInfo)}: Getting file info for file = {filePath}");
            try
            {
                return Task.Run(() =>
                {
                    var process = new Process
                    {
                        EnableRaisingEvents = true,
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = FileUtils.GetFFprobePath(),
                            UseShellExecute = false,
                            RedirectStandardInput = true,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true,
                            WindowStyle = ProcessWindowStyle.Hidden
                        }
                    };
                    process.StartInfo.Arguments = @$"-v quiet -print_format json -show_streams -show_format -i ""{filePath}""";
                    process.Start();
                    string json = process.StandardOutput.ReadToEnd();
                    return JsonConvert.DeserializeObject<FFProbeFileInfo>(json);
                }, token);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"{nameof(GetFileInfo)}: Unknown error");
                return Task.FromResult<FFProbeFileInfo>(null);
            }
        }

        public async Task TranscodeVideo(
            Stream outputStream,
            string filePath,
            int videoStreamIndex,
            int audioStreamIndex,
            double seconds,
            CancellationToken token)
        {
            var ffprobeFileInfo = await GetFileInfo(filePath, token);
            var videoInfo = ffprobeFileInfo.Streams.Find(f => f.IsVideo);
            var audioInfo = ffprobeFileInfo.Streams.Find(f => f.IsAudio);
            string fileExt = Path.GetExtension(filePath);

            bool videoCodecIsValid = videoInfo.VideoCodecIsValid(AllowedVideoCodecs);
            bool videoLevelIsValid = videoInfo.VideoLevelIsValid(MaxVideoLevel);
            bool videoProfileIsValid = videoInfo.VideoProfileIsValid(AllowedVideoProfiles);

            bool videoNeedsTranscode = !videoCodecIsValid || !videoProfileIsValid || !videoLevelIsValid;
            bool audioNeedsTranscode = !audioInfo.AudioCodecIsValid(AllowedMusicCodecs) || !audioInfo.AudioProfileIsValid(AllowedMusicProfiles);

            string cmd = string.Format(
                $@"-v quiet -ss {seconds} -y -i ""{filePath}"" -map 0:{videoStreamIndex} -map 0:{audioStreamIndex} -preset ultrafast {{0}} {{1}} -movflags frag_keyframe+faststart -f mp4 -",
                videoNeedsTranscode ? "-c:v libx264 -profile:v high -level 4" : "-c:v copy",
                audioNeedsTranscode ? "-acodec aac -b:a 128k" : "-c:a copy");

            //https://forums.plex.tv/t/best-format-for-streaming-via-chromecast/49978/6
            //ffmpeg - i[inputfile] - c:v libx264 -profile:v high -level 5 - crf 18 - maxrate 10M - bufsize 16M - pix_fmt yuv420p - vf "scale=iw*sar:ih, scale='if(gt(iw,ih),min(1920,iw),-1)':'if(gt(iw,ih),-1,min(1080,ih))'" - x264opts bframes = 3:cabac = 1 - movflags faststart - strict experimental - c:a aac -b:a 320k - y[outputfile]
            //string cmd = $@"-v quiet -ss {seconds} -y -i ""{filePath}"" -preset superfast -c:v copy -acodec aac -b:a 128k -movflags frag_keyframe+faststart -f mp4 -";
            _logger.Info($"{nameof(TranscodeVideo)}: Trying to transcode file = {filePath} with CMD = {cmd}");
            _transcodeProcess.StartInfo.Arguments = cmd;
            _transcodeProcess.Start();

            var stream = _transcodeProcess.StandardOutput.BaseStream as FileStream;
            await stream.CopyToAsync(outputStream, token);
            _logger.Info($"{nameof(TranscodeVideo)}: Transcode completed for file = {filePath}");
        }

        public async Task TranscodeMusic(IHttpContext context, string filePath, int audioStreamIndex, double seconds, CancellationToken token)
        {
            var ffprobeFileInfo = await GetFileInfo(filePath, token);
            var audioInfo = ffprobeFileInfo.Streams.Find(f => f.IsAudio);
            bool audioNeedsTranscode = !audioInfo.AudioCodecIsValid(AllowedMusicCodecs) || !audioInfo.AudioProfileIsValid(AllowedMusicProfiles);
            //To generate an aac output you need to set adts
            string cmd = string.Format(
                $@"-v quiet -ss {seconds} -y -i ""{filePath}"" -map 0:{audioStreamIndex} -preset ultrafast {{0}} -f adts -",
                audioNeedsTranscode ? "-acodec aac -b:a 128k" : "-c:a copy");

            _logger.Info($"{nameof(TranscodeMusic)}: Trying to transcode file = {filePath} with CMD = {cmd}");
            _transcodeProcess.StartInfo.Arguments = cmd;
            _transcodeProcess.Start();
            using var memoryStream = new MemoryStream();
            var stream = _transcodeProcess.StandardOutput.BaseStream as FileStream;
            await stream.CopyToAsync(memoryStream, token);
            memoryStream.Position = 0;
            //TODO: THIS LENGTH IS NOT WORKING PROPERLY
            context.Response.ContentLength64 = memoryStream.Length;
            await memoryStream.CopyToAsync(context.Response.OutputStream, token);
            _logger.Info($"{nameof(TranscodeMusic)}: Transcode completed for file = {filePath}");
        }

        public string GetOutputTranscodeMimeType(string filepath)
        {
            bool isVideoFile = FileUtils.IsVideoFile(filepath);
            bool isMusicFile = FileUtils.IsMusicFile(filepath);

            if (isVideoFile || isMusicFile)
            {
                //The transcode process generate either of these
                return isVideoFile ? "video/mp4" : "audio/aac";
            }
            return "video/webm";
        }

        public Task GenerateSubTitles(
            string filePath,
            string subtitleFinalPath,
            double seconds,
            int index,
            CancellationToken token)
        {
            try
            {
                string cmd = @$"-y -i ""{filePath}"" -ss {seconds} -map 0:{index} -f webvtt ""{subtitleFinalPath}""";
                _logger.Info($"{nameof(GenerateSubTitles)}: Generating subtitles for file = {filePath}. CMD = {cmd}");
                return Task.Run(() =>
                {
                    var process = new Process
                    {
                        EnableRaisingEvents = true,
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = FileUtils.GetFFMpegPath(),
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            WindowStyle = ProcessWindowStyle.Hidden
                        }
                    };
                    process.StartInfo.Arguments = cmd;
                    process.Start();
                    process.WaitForExit();
                }, token);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"{nameof(GenerateSubTitles)}: Unknown error");
                return Task.FromResult<string>(null);
            }
        }
    }
}
