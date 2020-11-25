using CastIt.Application.Common;
using CastIt.Application.Interfaces;
using CastIt.Domain.Enums;
using CastIt.Domain.Exceptions;
using CastIt.Domain.Models.FFmpeg.Args;
using CastIt.Domain.Models.FFmpeg.Info;
using CastIt.Domain.Models.FFmpeg.Transcode;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;

namespace CastIt.Application.FFMpeg
{
    public class FFmpegService : IFFmpegService
    {
        private readonly ILogger<FFmpegService> _logger;
        private readonly IFileService _fileService;
        private readonly ITelemetryService _telemetryService;

        private readonly Process _generateThumbnailProcess;
        private readonly Process _generateAllThumbnailsProcess;
        private readonly Process _transcodeProcess;
        private readonly List<HwAccelDeviceType> _availableHwDevices = new List<HwAccelDeviceType>();

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
            => $"{FileFormatConstants.ThumbnailWidth}:{FileFormatConstants.ThumbnailHeight}";

        private bool _checkGenerateThumbnailProcess;
        private bool _checkGenerateAllThumbnailsProcess;

        public FFmpegService(
            ILogger<FFmpegService> logger,
            IFileService fileService,
            ITelemetryService telemetryService)
        {
            _logger = logger;
            _fileService = fileService;
            _telemetryService = telemetryService;
            _transcodeProcess = new Process
            {
                EnableRaisingEvents = true,
                StartInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = fileService.GetFFmpegPath(),
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            var startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = fileService.GetFFmpegPath(),
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

        public void Init(bool isHwAccelIsEnabled)
        {
            SetAvailableHwAccelDevices(isHwAccelIsEnabled);
        }

        public string GetThumbnail(string mrl)
        {
            if (!_fileService.IsLocalFile(mrl))
            {
                _logger.LogWarning($"{nameof(GetThumbnail)}: Cant get thumbnail for file = {mrl}. Its not a local file");
                return null;
            }

            var filename = Path.GetFileName(mrl);
            var thumbnailPath = _fileService.GetThumbnailFilePath(filename, 0);
            if (File.Exists(thumbnailPath))
            {
                return thumbnailPath;
            }

            var builder = new FFmpegArgsBuilder();
            builder.AddInputFile(mrl).BeQuiet().SetAutoConfirmChanges().DisableAudio();
            var outputArgs = builder.AddOutputFile(thumbnailPath);
            if (!_fileService.IsMusicFile(mrl))
            {
                outputArgs.WithVideoFilter(@"select=gt(scene\,0.4)")
                    .SetVideoFrames(1)
                    .WithVideoFilter("fps=1/60");
            }
            try
            {
                string cmd = builder.GetArgs();
                _logger.LogInformation($"{nameof(GetThumbnail)}: Generating first thumbnail for file = {mrl}. Cmd = {cmd}");
                _checkGenerateThumbnailProcess = true;
                _generateThumbnailProcess.StartInfo.Arguments = cmd;
                _generateThumbnailProcess.Start();
                _generateThumbnailProcess.WaitForExit();
                if (_generateThumbnailProcess.ExitCode != 0)
                {
                    _logger.LogWarning($"{nameof(GetThumbnail)}: Couldn't retrieve the first thumbnail for file = {mrl}.");
                    return null;
                }
                _logger.LogInformation($"{nameof(GetThumbnail)}: First thumbnail was successfully generated for file = {mrl}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(GenerateThumbnails)}: Unknown error occurred");
                _telemetryService.TrackError(ex);
            }
            finally
            {
                _checkGenerateThumbnailProcess = false;
            }
            return thumbnailPath;
        }

        public async Task GenerateThumbnails(string mrl, bool hwAccelIsEnabled)
        {
            if (!_fileService.IsLocalFile(mrl))
            {
                _logger.LogInformation($"{nameof(GenerateThumbnails)}: File = {mrl} is not a local file, so we wont generate thumbnails for it");
                return;
            }
            var filename = Path.GetFileName(mrl);

            if (_fileService.IsMusicFile(mrl))
            {
                _logger.LogInformation($"{nameof(GenerateThumbnails)}: File = {mrl} is a music one, so we wont generate thumbnails for it");
                return;
            }
            var fileInfo = await GetFileInfo(mrl, default).ConfigureAwait(false);
            if (fileInfo == null || fileInfo?.Videos.Any(f => f.IsVideo) == false)
                throw new InvalidOperationException($"The file = {mrl} does not have a valid file info or video stream");

            string thumbnailPath = _fileService.GetPreviewThumbnailFilePath(filename);
            bool useHwAccel = fileInfo.Videos.Find(f => f.IsVideo).VideoCodecIsValid(AllowedVideoCodecs) &&
                _availableHwDevices.Contains(HwAccelDeviceType.Intel) &&
                hwAccelIsEnabled;

            string cmd = GenerateCmdForThumbnails(mrl, thumbnailPath, useHwAccel);
            try
            {
                _logger.LogInformation($"{nameof(GenerateThumbnails)}: Generating all thumbnails for file = {mrl}. Cmd = {cmd}");
                _checkGenerateAllThumbnailsProcess = true;
                _generateAllThumbnailsProcess.StartInfo.Arguments = cmd;
                _generateAllThumbnailsProcess.Start();
                await _generateAllThumbnailsProcess.WaitForExitAsync();
                if (_generateAllThumbnailsProcess.ExitCode != 0 && useHwAccel)
                {
                    _logger.LogWarning(
                        $"{nameof(GenerateThumbnails)}: Could not generate thumbnails for file = {mrl} " +
                        "using hw accel, falling back to sw.");
                    cmd = GenerateCmdForThumbnails(mrl, thumbnailPath, false);

                    _generateAllThumbnailsProcess.StartInfo.Arguments = cmd;
                    _generateAllThumbnailsProcess.Start();
                    await _generateAllThumbnailsProcess.WaitForExitAsync();
                }

                if (_generateAllThumbnailsProcess.ExitCode != 0)
                {
                    _logger.LogError($"{nameof(GenerateThumbnails)}: Could not generate thumbnails for file = {mrl}.");
                    throw new FFmpegException("Couldn't generate thumbnails", cmd);
                }

                _logger.LogInformation($"{nameof(GenerateThumbnails)}: All thumbnails were successfully generated for file = {mrl}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(GenerateThumbnails)}: Unknown error occurred");
                _telemetryService.TrackError(ex);
            }
            finally
            {
                _checkGenerateAllThumbnailsProcess = false;
            }
        }

        public void KillThumbnailProcess()
        {
            _logger.LogInformation($"{nameof(KillThumbnailProcess)}: Killing thumbnail process");
            try
            {
                if (_checkGenerateAllThumbnailsProcess &&
                    !_generateAllThumbnailsProcess.HasExited)
                {
                    _logger.LogInformation($"{nameof(KillThumbnailProcess)}: Killing the generate all thumbnails process");
                    _generateAllThumbnailsProcess.Kill(true);
                    _generateAllThumbnailsProcess.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                if (ex is InvalidOperationException)
                    return;
                _logger.LogError(ex, $"{nameof(KillThumbnailProcess)}: Could not stop the generate all thumbnails process");
                _telemetryService.TrackError(ex);
            }

            try
            {
                if (_checkGenerateThumbnailProcess &&
                    !_generateThumbnailProcess.HasExited)
                {
                    _logger.LogInformation($"{nameof(KillThumbnailProcess)}: Killing the generate thumbnail process");
                    _generateThumbnailProcess.Kill(true);
                    _generateThumbnailProcess.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                if (ex is InvalidOperationException)
                    return;
                _logger.LogError(ex, $"{nameof(KillThumbnailProcess)}: Could not stop the generate thumbnail process");
                _telemetryService.TrackError(ex);
            }
        }

        public void KillTranscodeProcess()
        {
            _logger.LogInformation($"{nameof(KillTranscodeProcess)}: Killing transcode process");
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
                _logger.LogError(e, $"{nameof(KillTranscodeProcess)}: Unknown error occurred");
                _telemetryService.TrackError(e);
            }
        }

        public Task<FFProbeFileInfo> GetFileInfo(string filePath, CancellationToken token)
        {
            string cmd = @$"-v quiet -print_format json -show_streams -show_format -i ""{filePath}""";
            _logger.LogTrace($"{nameof(GetFileInfo)}: Getting file info for file = {filePath}. Cmd = {cmd}");
            try
            {
                if (!_fileService.Exists(filePath))
                {
                    _logger.LogWarning($"{nameof(GetFileInfo)}: File = {filePath} does not exist");
                    return Task.FromResult<FFProbeFileInfo>(null);
                }

                return Task.Run(() =>
                {
                    var process = new Process
                    {
                        EnableRaisingEvents = true,
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = _fileService.GetFFprobePath(),
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
                _logger.LogError(e, $"{nameof(GetFileInfo)}: Unknown error");
                _telemetryService.TrackError(e);
                return Task.FromResult<FFProbeFileInfo>(null);
            }
        }

        public async Task TranscodeVideo(Stream outputStream, TranscodeVideoFile options, CancellationToken token)
        {
            string cmd;
            if (options.HwAccelDeviceType != HwAccelDeviceType.None)
            {
                cmd = BuildVideoTranscodeCmd(options, 0, 2);

                _logger.LogInformation($"{nameof(TranscodeVideo)}: Checking if we can use the default cmd for hwAccelType = {options.HwAccelDeviceType}...");
                _transcodeProcess.StartInfo.Arguments = cmd;
                _transcodeProcess.Start();
                await using var memStream = new MemoryStream();
                var testStream = _transcodeProcess.StandardOutput.BaseStream as FileStream;
                await testStream.CopyToAsync(memStream, token).ConfigureAwait(false);
                if (_transcodeProcess.ExitCode != 0)
                {
                    _logger.LogWarning(
                        $"{nameof(TranscodeVideo)}: We cant use the default cmd = {cmd} for hwAccelType = {options.HwAccelDeviceType}. " +
                        $"Creating a new one with hwAccelType = {HwAccelDeviceType.None}..");
                    options.HwAccelDeviceType = HwAccelDeviceType.None;
                }
            }

            cmd = BuildVideoTranscodeCmd(options);

            //https://forums.plex.tv/t/best-format-for-streaming-via-chromecast/49978/6
            //ffmpeg - i[inputfile] - c:v libx264 -profile:v high -level 5 - crf 18 - maxrate 10M - bufsize 16M - pix_fmt yuv420p - vf "scale=iw*sar:ih, scale='if(gt(iw,ih),min(1920,iw),-1)':'if(gt(iw,ih),-1,min(1080,ih))'" - x264opts bframes = 3:cabac = 1 - movflags faststart - strict experimental - c:a aac -b:a 320k - y[outputfile]
            //string cmd = $@"-v quiet -ss {seconds} -y -i ""{filePath}"" -preset superfast -c:v copy -acodec aac -b:a 128k -movflags frag_keyframe+faststart -f mp4 -";
            _logger.LogInformation($"{nameof(TranscodeVideo)}: Trying to transcode file with CMD = {cmd}");
            _transcodeProcess.StartInfo.Arguments = cmd;
            _transcodeProcess.Start();
            var stream = _transcodeProcess.StandardOutput.BaseStream as FileStream;
            await stream.CopyToAsync(outputStream, token).ConfigureAwait(false);
            _logger.LogInformation($"{nameof(TranscodeVideo)}: Transcode completed for file = {options.FilePath}");
        }

        public async Task<MemoryStream> TranscodeMusic(TranscodeMusicFile options, CancellationToken token)
        {
            var fileInfo = await GetFileInfo(options.FilePath, token).ConfigureAwait(false);
            if (fileInfo == null)
            {
                _logger.LogInformation($"{nameof(TranscodeVideo)}: Couldn't retrieve file info for file = {options.FilePath}");
                return null;
            }
            var audioInfo = fileInfo.Streams.Find(f => f.IsAudio);
            if (audioInfo is null)
                throw new NullReferenceException($"The file = {options.FilePath} does not have a valid audio stream");

            bool audioNeedsTranscode = !audioInfo.AudioCodecIsValid(AllowedMusicCodecs) ||
                !audioInfo.AudioProfileIsValid(AllowedMusicProfiles) ||
                options.ForceAudioTranscode;

            string cmd = BuildAudioTranscodeCmd(options.FilePath, options.Seconds, options.AudioStreamIndex, audioNeedsTranscode);

            _logger.LogInformation($"{nameof(TranscodeMusic)}: Trying to transcode file with CMD = {cmd}");
            _transcodeProcess.StartInfo.Arguments = cmd;
            _transcodeProcess.Start();
            var memoryStream = new MemoryStream();
            var stream = _transcodeProcess.StandardOutput.BaseStream as FileStream;
            await stream.CopyToAsync(memoryStream, token).ConfigureAwait(false);

            memoryStream.Position = 0;
            _logger.LogInformation($"{nameof(TranscodeMusic)}: Transcode completed for file = {options.FilePath}");

            return memoryStream;
        }

        public string GetOutputTranscodeMimeType(string filepath)
        {
            bool isVideoFile = _fileService.IsVideoFile(filepath);
            bool isMusicFile = _fileService.IsMusicFile(filepath);
            bool isHls = _fileService.IsHls(filepath);
            if (isVideoFile || isMusicFile || isHls)
            {
                //The transcode process generate either of these
                return isVideoFile || isHls ? "video/mp4" : "audio/aac";
            }

            return "video/webm";
        }

        public Task GenerateSubTitles(
            string filePath,
            string subtitleFinalPath,
            double seconds,
            int index,
            double subsDelayInSeconds,
            CancellationToken token)
        {
            try
            {
                _fileService.DeleteFilesInDirectory(Path.GetDirectoryName(subtitleFinalPath));
                var builder = new FFmpegArgsBuilder();
                builder.AddInputFile(filePath).BeQuiet().SetAutoConfirmChanges().TrySetSubTitleEncoding(FileFormatConstants.AllowedSubtitleFormats);
                builder.AddOutputFile(subtitleFinalPath)
                    .Seek(Math.Floor(seconds))
                    .SetMap(index)
                    .SetDelayInSeconds(subsDelayInSeconds)
                    .SetFormat("webvtt");

                string cmd = builder.GetArgs();
                _logger.LogInformation($"{nameof(GenerateSubTitles)}: Generating subtitles for file = {filePath}. CMD = {cmd}");
                return Task.Run(() =>
                {
                    var process = new Process
                    {
                        EnableRaisingEvents = true,
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = _fileService.GetFFmpegPath(),
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            WindowStyle = ProcessWindowStyle.Hidden
                        }
                    };
                    process.StartInfo.Arguments = cmd;
                    process.Start();
                    process.WaitForExit();

                    if (process.ExitCode > 0)
                    {
                        _logger.LogError($"{nameof(GenerateSubTitles)}: Could not generate subs for file = {filePath}.");
                    }
                    else if (process.ExitCode < 0)
                    {
                        _logger.LogInformation($"{nameof(GenerateSubTitles)}: Process was killed for file = {filePath}");
                    }
                    else
                    {
                        _logger.LogInformation($"{nameof(GenerateSubTitles)}: Subs where generated for file = {filePath}");
                    }
                }, token);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(GenerateSubTitles)}: Unknown error");
                _telemetryService.TrackError(e);
                return Task.CompletedTask;
            }
        }

        public bool VideoNeedsTranscode(int videoStreamIndex, bool forceVideoTranscode, VideoScaleType selectedScale, FFProbeFileInfo fileInfo)
        {
            if (fileInfo == null)
                throw new ArgumentNullException(nameof(fileInfo), "The provided file info can't be null");

            var videoInfo = fileInfo.Videos.FirstOrDefault(f => f.Index == videoStreamIndex && f.IsVideo);
            if (videoInfo is null)
                throw new NullReferenceException("The file does not have a valid video stream");

            bool videoCodecIsValid = videoInfo.VideoCodecIsValid(AllowedVideoCodecs);
            bool videoLevelIsValid = videoInfo.VideoLevelIsValid(MaxVideoLevel);
            bool videoProfileIsValid = videoInfo.VideoProfileIsValid(AllowedVideoProfiles);

            return !videoCodecIsValid ||
               !videoProfileIsValid ||
               !videoLevelIsValid ||
               forceVideoTranscode ||
               selectedScale != VideoScaleType.Original;
        }

        public bool AudioNeedsTranscode(int audioStreamIndex, bool forceAudioTranscode, FFProbeFileInfo fileInfo, bool checkIfAudioIsNull = false)
        {
            if (fileInfo == null)
                throw new ArgumentNullException(nameof(fileInfo), "The provided file info can't be null");

            var audioInfo = fileInfo.Streams.FirstOrDefault(f => f.Index == audioStreamIndex && f.IsAudio);
            if (checkIfAudioIsNull && audioInfo is null)
                throw new NullReferenceException("The file does not have a valid audio stream");

            return audioInfo != null &&
               (!audioInfo.AudioCodecIsValid(AllowedMusicCodecs) ||
                !audioInfo.AudioProfileIsValid(AllowedMusicProfiles) ||
                forceAudioTranscode);
        }

        public HwAccelDeviceType GetHwAccelToUse(
            int videoStreamIndex,
            FFProbeFileInfo fileInfo,
            bool shouldUseHwAccel = true,
            bool isHls = false)
        {
            if (fileInfo == null)
                throw new ArgumentNullException(nameof(fileInfo), "The provided file info can't be null");

            var videoInfo = fileInfo.Videos.FirstOrDefault(f => f.Index == videoStreamIndex && f.IsVideo);
            if (videoInfo is null)
                throw new NullReferenceException($"The file does not have a valid video stream");

            bool videoCodecIsValid = videoInfo.VideoCodecIsValid(AllowedVideoCodecs);
            var hwAccelType = GetHwAccelDeviceType();
            if (!videoCodecIsValid || !shouldUseHwAccel || isHls)
            {
                hwAccelType = HwAccelDeviceType.None;
            }

            return hwAccelType;
        }

        private static string BuildVideoTranscodeCmd(
            TranscodeVideoFile options,
            double? from = null,
            double? to = null)
        {
            var builder = new FFmpegArgsBuilder();
            var inputArgs = builder.AddInputFile(options.FilePath)
                .BeQuiet()
                .SetVSync(0)
                .SetAutoConfirmChanges()
                .SetHwAccel(options.HwAccelDeviceType)
                .SetVideoCodec(options.HwAccelDeviceType);
            var outputArgs = builder.AddOutputPipe()
                .SetPreset(options.HwAccelDeviceType)
                .AddArg("map_metadata", -1)
                .AddArg("map_chapters", -1); //for some reason the chromecast doesnt like chapters o.o

            if (options.VideoStreamIndex >= 0)
                outputArgs.SetMap(options.VideoStreamIndex);

            if (options.AudioStreamIndex >= 0)
                outputArgs.SetMap(options.AudioStreamIndex);

            if (options.Seconds > 0 && !from.HasValue && !to.HasValue)
                inputArgs.Seek(options.Seconds);

            if (from.HasValue)
                outputArgs.Seek(from.Value);

            if (to.HasValue)
                outputArgs.To(to.Value);

            if (options.ForceVideoTranscode)
            {
                outputArgs.SetVideoCodec(options.HwAccelDeviceType).SetProfileVideo("main").SetLevel(4).SetPixelFormat(options.HwAccelDeviceType);
                if (options.VideoScaleType != VideoScaleType.Original)
                {
                    int videoScale = (int)options.VideoScaleType;
                    switch (options.HwAccelDeviceType)
                    {
                        case HwAccelDeviceType.Intel:
                            outputArgs.AddArg("global_quality", 25)
                                .AddArg("look_ahead", 1)
                                .SetFilters($"scale_qsv=trunc(oh*a/2)*2:{videoScale}");
                            break;
                        case HwAccelDeviceType.Nvidia:
                            string scale = options.VideoScaleType == VideoScaleType.Hd ? "1280x720" : "1920x1080";
                            if (scale != options.VideoWidthAndHeight && !string.IsNullOrWhiteSpace(options.VideoWidthAndHeight))
                                inputArgs.AddArg("resize", scale);
                            break;
                        case HwAccelDeviceType.None:
                            outputArgs.SetFilters($"scale=trunc(oh*a/2)*2:{videoScale}");
                            break;
                        case HwAccelDeviceType.AMD:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            else
            {
                outputArgs.CopyVideoCodec();
            }

            if (options.ForceAudioTranscode)
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

        private void SetAvailableHwAccelDevices(bool isHwAccelIsEnabled)
        {
            //TODO: IMPROVE THIS
            if (!isHwAccelIsEnabled)
                return;
            try
            {
                if (!OperatingSystem.IsWindows())
                {
                    _logger.LogInformation($"{nameof(SetAvailableHwAccelDevices)}: Not running on windows, nothing to do here");
                    return;
                }
                _logger.LogInformation($"{nameof(SetAvailableHwAccelDevices)}: Getting available devices..");
                var objvide = new ManagementObjectSearcher("select * from Win32_VideoController");
                var adapters = new List<string>();
                foreach (var o in objvide.Get())
                {
                    var obj = (ManagementObject)o;
                    adapters.Add(obj["AdapterCompatibility"].ToString());
                }

                if (adapters.Any(a => a.Contains(nameof(HwAccelDeviceType.Nvidia), StringComparison.OrdinalIgnoreCase)))
                {
                    _availableHwDevices.Add(HwAccelDeviceType.Nvidia);
                }

                if (adapters.Any(a => a.Contains(nameof(HwAccelDeviceType.AMD), StringComparison.OrdinalIgnoreCase)))
                {
                    _availableHwDevices.Add(HwAccelDeviceType.AMD);
                }

                if (adapters.Any(a => a.Contains(nameof(HwAccelDeviceType.Intel), StringComparison.OrdinalIgnoreCase)))
                {
                    _availableHwDevices.Add(HwAccelDeviceType.Intel);
                }

                _logger.LogInformation($"{nameof(SetAvailableHwAccelDevices)}: Got the following devices: {string.Join(",", _availableHwDevices)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(SetAvailableHwAccelDevices)}: Unknown error");
                _telemetryService.TrackError(ex);
            }
        }

        private HwAccelDeviceType GetHwAccelDeviceType()
        {
            if (_availableHwDevices.Contains(HwAccelDeviceType.Nvidia))
                return HwAccelDeviceType.Nvidia;

            if (_availableHwDevices.Contains(HwAccelDeviceType.AMD))
                return HwAccelDeviceType.AMD;

            return _availableHwDevices.Contains(HwAccelDeviceType.Intel) ? HwAccelDeviceType.Intel : HwAccelDeviceType.None;
        }

        private string GenerateCmdForThumbnails(string mrl, string thumbnailPath, bool useHwAccel)
        {
            var builder = new FFmpegArgsBuilder();
            var inputArgs = builder.AddInputFile(mrl).SetAutoConfirmChanges().DisableAudio();
            var outputArgs = builder.AddOutputFile(thumbnailPath);

            if (useHwAccel)
            {
                //This might fail sometimes because the device does not qsv or because the main display is not connected to the Intel GPU
                inputArgs.SetHwAccel(HwAccelDeviceType.Intel).SetVideoCodec(HwAccelDeviceType.Intel);
                outputArgs.SetVideoCodec("mjpeg_qsv").SetFilters($"fps=1/5,scale_qsv={ThumbnailScale}");
            }
            else
            {
                outputArgs.SetFilters($"fps=1/5,scale={ThumbnailScale}");
            }

            return builder.GetArgs();
        }
    }
}
