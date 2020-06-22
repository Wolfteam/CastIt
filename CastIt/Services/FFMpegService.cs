using CastIt.Common;
using CastIt.Common.Enums;
using CastIt.Common.Utils;
using CastIt.Interfaces;
using CastIt.Models.FFMpeg;
using CastIt.Models.FFMpeg.Args;
using EmbedIO;
using MvvmCross.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;

namespace CastIt.Services
{
    public class FFMpegService : IFFMpegService
    {
        private readonly IMvxLog _logger;
        private readonly IAppSettingsService _settingsService;
        private readonly ITelemetryService _telemetryService;

        private readonly Process _generateThumbnailProcess;
        private readonly Process _generateAllThumbnailsProcess;
        private readonly Process _transcodeProcess;
        private readonly List<HwAccelDeviceType> AvailableHwDevices = new List<HwAccelDeviceType>();

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

        private static string ThumbnailScale
            => $"{AppConstants.ThumbnailWidth}:{AppConstants.ThumbnailHeight}";

        private bool _checkGenerateThumbnailProcess;
        private bool _checkGenerateAllThumbnailsProcess;

        public FFMpegService(
            IMvxLogProvider logProvider,
            IAppSettingsService settingsService,
            ITelemetryService telemetryService)
        {
            _logger = logProvider.GetLogFor<FFMpegService>();
            _settingsService = settingsService;
            _telemetryService = telemetryService;
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
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = FileUtils.GetFFMpegPath(),
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

            SetAvailableHwAccelDevices();
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

            var builder = new FFmpegArgsBuilder();
            var inputArgs = builder.AddInputFile(mrl).BeQuiet().SetAutoConfirmChanges().DisableAudio();
            var ouputArgs = builder.AddOutputFile(thumbnailPath);
            if (!FileUtils.IsMusicFile(mrl))
            {
                inputArgs.Seek(second);
                ouputArgs.SetVideoFrames(1);
            }
            try
            {
                string cmd = builder.GetArgs();
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
                _telemetryService.TrackError(ex);
            }
            finally
            {
                _checkGenerateThumbnailProcess = false;
            }
            return thumbnailPath;
        }

        public async Task GenerateThumbmnails(string mrl)
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
            var fileInfo = await GetFileInfo(mrl, default);
            var thumbnailPath = FileUtils.GetPreviewThumbnailFilePath(filename);
            var builder = new FFmpegArgsBuilder();
            var inputArgs = builder.AddInputFile(mrl).SetAutoConfirmChanges().DisableAudio();
            var outputArgs = builder.AddOutputFile(thumbnailPath);

            if (fileInfo.Videos.Find(f => f.IsVideo).VideoCodecIsValid(AllowedVideoCodecs) &&
                AvailableHwDevices.Contains(HwAccelDeviceType.Intel))
            {
                inputArgs.SetHwAccel(HwAccelDeviceType.Intel).SetVideoCodec(HwAccelDeviceType.Intel);
                outputArgs.SetVideoCodec("mjpeg_qsv").SetFilters($"fps=1/5,scale_qsv={ThumbnailScale}");
            }
            else
            {
                outputArgs.SetFilters($"fps=1/5,scale={ThumbnailScale}");
            }

            var cmd = builder.GetArgs();
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
                _telemetryService.TrackError(ex);
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
                _telemetryService.TrackError(ex);
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
                _telemetryService.TrackError(ex);
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
                _telemetryService.TrackError(e);
            }
        }

        public Task<FFProbeFileInfo> GetFileInfo(string filePath, CancellationToken token)
        {
            string cmd = @$"-v quiet -print_format json -show_streams -show_format -i ""{filePath}""";
            _logger.Trace($"{nameof(GetFileInfo)}: Getting file info for file = {filePath}. Cmd = {cmd}");
            try
            {
                if (!File.Exists(filePath))
                {
                    return Task.FromResult<FFProbeFileInfo>(null);
                }
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
                    process.StartInfo.Arguments = cmd;
                    process.Start();
                    string json = process.StandardOutput.ReadToEnd();
                    return JsonConvert.DeserializeObject<FFProbeFileInfo>(json);
                }, token);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"{nameof(GetFileInfo)}: Unknown error");
                _telemetryService.TrackError(e);
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
            var ffprobeFileInfo = await GetFileInfo(filePath, token).ConfigureAwait(false);
            string cmd = BuildVideoTranscodeCmd(ffprobeFileInfo, filePath, seconds, videoStreamIndex, audioStreamIndex);

            //https://forums.plex.tv/t/best-format-for-streaming-via-chromecast/49978/6
            //ffmpeg - i[inputfile] - c:v libx264 -profile:v high -level 5 - crf 18 - maxrate 10M - bufsize 16M - pix_fmt yuv420p - vf "scale=iw*sar:ih, scale='if(gt(iw,ih),min(1920,iw),-1)':'if(gt(iw,ih),-1,min(1080,ih))'" - x264opts bframes = 3:cabac = 1 - movflags faststart - strict experimental - c:a aac -b:a 320k - y[outputfile]
            //string cmd = $@"-v quiet -ss {seconds} -y -i ""{filePath}"" -preset superfast -c:v copy -acodec aac -b:a 128k -movflags frag_keyframe+faststart -f mp4 -";
            _logger.Info($"{nameof(TranscodeVideo)}: Trying to transcode file = {filePath} with CMD = {cmd}");
            _transcodeProcess.StartInfo.Arguments = cmd;
            _transcodeProcess.Start();
            var stream = _transcodeProcess.StandardOutput.BaseStream as FileStream;
            await stream.CopyToAsync(outputStream, token).ConfigureAwait(false);
            _logger.Info($"{nameof(TranscodeVideo)}: Transcode completed for file = {filePath}");
        }

        public async Task TranscodeMusic(IHttpContext context, string filePath, int audioStreamIndex, double seconds, CancellationToken token)
        {
            var ffprobeFileInfo = await GetFileInfo(filePath, token).ConfigureAwait(false);
            var audioInfo = ffprobeFileInfo.Streams.Find(f => f.IsAudio);
            bool audioNeedsTranscode = !audioInfo.AudioCodecIsValid(AllowedMusicCodecs) ||
                !audioInfo.AudioProfileIsValid(AllowedMusicProfiles) ||
                _settingsService.ForceAudioTranscode;

            string cmd = BuildAudioTranscodeCmd(filePath, seconds, audioStreamIndex, audioNeedsTranscode);

            _logger.Info($"{nameof(TranscodeMusic)}: Trying to transcode file = {filePath} with CMD = {cmd}");
            _transcodeProcess.StartInfo.Arguments = cmd;
            _transcodeProcess.Start();
            using var memoryStream = new MemoryStream();
            var stream = _transcodeProcess.StandardOutput.BaseStream as FileStream;
            await stream.CopyToAsync(memoryStream, token).ConfigureAwait(false);
            memoryStream.Position = 0;
            //TODO: THIS LENGTH IS NOT WORKING PROPERLY
            context.Response.ContentLength64 = memoryStream.Length;
            await memoryStream.CopyToAsync(context.Response.OutputStream, token).ConfigureAwait(false);
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
                FileUtils.DeleteFilesInDirectory(Path.GetDirectoryName(subtitleFinalPath));
                var builder = new FFmpegArgsBuilder();
                builder.AddInputFile(filePath).BeQuiet().SetAutoConfirmChanges().TrySetSubTitleEncoding();
                builder.AddOutputFile(subtitleFinalPath).Seek(seconds).SetMap(index).SetFormat("webvtt");

                string cmd = builder.GetArgs();
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
                _telemetryService.TrackError(e);
                return Task.CompletedTask;
            }
        }

        private string BuildVideoTranscodeCmd(
            FFProbeFileInfo fileInfo,
            string filePath,
            double seconds,
            int videoStreamIndex,
            int audioStreamIndex)
        {
            string fileExt = Path.GetExtension(filePath);
            var videoInfo = fileInfo.Streams.Find(f => f.IsVideo);
            var audioInfo = fileInfo.Streams.Find(f => f.IsAudio);
            bool videoCodecIsValid = videoInfo.VideoCodecIsValid(AllowedVideoCodecs);
            bool videoLevelIsValid = videoInfo.VideoLevelIsValid(MaxVideoLevel);
            bool videoProfileIsValid = videoInfo.VideoProfileIsValid(AllowedVideoProfiles);

            bool videoNeedsTranscode = !videoCodecIsValid ||
                !videoProfileIsValid ||
                !videoLevelIsValid ||
                _settingsService.ForceVideoTranscode ||
                _settingsService.VideoScale != VideoScaleType.Original;
            bool audioNeedsTranscode = !audioInfo.AudioCodecIsValid(AllowedMusicCodecs) ||
                !audioInfo.AudioProfileIsValid(AllowedMusicProfiles) ||
                _settingsService.ForceAudioTranscode;

            var hwAccelType = GetHwAccelDeviceType();
            if (!videoCodecIsValid)
            {
                hwAccelType = HwAccelDeviceType.None;
            }

            var builder = new FFmpegArgsBuilder();
            var inputArgs = builder.AddInputFile(filePath)
                .BeQuiet()
                .SetVSync(0)
                .SetAutoConfirmChanges()
                .SetHwAccel(hwAccelType)
                .SetVideoCodec(hwAccelType);
            var outputArgs = builder.AddOutputPipe()
                .SetMap(videoStreamIndex)
                .SetMap(audioStreamIndex)
                .SetPreset(hwAccelType)
                .AddArg("map_metadata", -1)
                .AddArg("map_chapters", -1); //for some reason the chromecast doesnt like chapters o.o

            if (seconds > 0)
                inputArgs.Seek(seconds);

            if (videoNeedsTranscode)
            {
                outputArgs.SetVideoCodec(hwAccelType).SetProfileVideo("main").SetLevel(4).SetPixelFormat(hwAccelType);
                if (_settingsService.VideoScale != VideoScaleType.Original)
                {
                    int videoScale = (int)_settingsService.VideoScale;
                    if (hwAccelType == HwAccelDeviceType.Intel)
                    {
                        outputArgs.AddArg("global_quality", 25);
                        outputArgs.AddArg("look_ahead", 1);
                        outputArgs.SetFilters($"scale_qsv=trunc(oh*a/2)*2:{videoScale}");
                    }
                    else if (hwAccelType == HwAccelDeviceType.Nvidia)
                    {
                        string scale = _settingsService.VideoScale == VideoScaleType.Hd ? "1280x720" : "1920x1080";
                        if (scale != videoInfo.WidthAndHeightText)
                            inputArgs.AddArg("resize", scale);
                    }
                    else if (hwAccelType == HwAccelDeviceType.None)
                    {
                        outputArgs.SetFilters($"scale=trunc(oh*a/2)*2:{videoScale}");
                    }
                }
            }
            else
            {
                outputArgs.CopyVideoCodec();
            }

            if (audioNeedsTranscode)
            {
                outputArgs.SetAudioCodec("aac").SetAudioBitrate(128).SetAudioChannels(2);
            }
            else
            {
                outputArgs.CopyAudioCodec();
            }

            outputArgs.SetMovFlagToTheStart().SetFormat("mp4");

            return builder.GetArgs();
        }

        private string BuildAudioTranscodeCmd(string filePath, double seconds, int audioStreamIndex, bool audioNeedsTranscode)
        {
            var builder = new FFmpegArgsBuilder();
            builder.AddInputFile(filePath).BeQuiet().SetAutoConfirmChanges().Seek(seconds);
            var outputArgs = builder.AddOutputPipe().SetMap(audioStreamIndex).SetPreset("ultrafast");

            if (audioNeedsTranscode)
            {
                outputArgs.SetAudioCodec("aac").SetAudioBitrate(128);
            }
            else
            {
                outputArgs.SetAudioCodec("copy");
            }
            //To generate an aac output you need to set adts
            outputArgs.SetAudioChannels(2).SetFormat("adts");

            return builder.GetArgs();
        }

        private void SetAvailableHwAccelDevices()
        {
            //TODO: IMPROVE THIS
            if (!_settingsService.EnableHardwareAcceleration)
                return;
            try
            {
                _logger.Info($"{nameof(SetAvailableHwAccelDevices)}: Getting available devices..");
                var objvide = new ManagementObjectSearcher("select * from Win32_VideoController");
                var adapters = new List<string>();
                foreach (ManagementObject obj in objvide.Get())
                {
                    adapters.Add(obj["AdapterCompatibility"].ToString());
                }

                if (adapters.Any(a => a.Contains(nameof(HwAccelDeviceType.Nvidia), StringComparison.OrdinalIgnoreCase)))
                {
                    AvailableHwDevices.Add(HwAccelDeviceType.Nvidia);
                }

                if (adapters.Any(a => a.Contains(nameof(HwAccelDeviceType.AMD), StringComparison.OrdinalIgnoreCase)))
                {
                    AvailableHwDevices.Add(HwAccelDeviceType.AMD);
                }

                if (adapters.Any(a => a.Contains(nameof(HwAccelDeviceType.Intel), StringComparison.OrdinalIgnoreCase)))
                {
                    AvailableHwDevices.Add(HwAccelDeviceType.Intel);
                }

                _logger.Info($"{nameof(SetAvailableHwAccelDevices)}: Got the following devices: {string.Join(",", AvailableHwDevices)}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"{nameof(SetAvailableHwAccelDevices)}: Unknown error");
                _telemetryService.TrackError(ex);
            }
        }

        private HwAccelDeviceType GetHwAccelDeviceType()
        {
            if (AvailableHwDevices.Contains(HwAccelDeviceType.Nvidia))
                return HwAccelDeviceType.Nvidia;

            if (AvailableHwDevices.Contains(HwAccelDeviceType.AMD))
                return HwAccelDeviceType.AMD;

            if (AvailableHwDevices.Contains(HwAccelDeviceType.Intel))
                return HwAccelDeviceType.Intel;

            return HwAccelDeviceType.None;
        }
    }
}
