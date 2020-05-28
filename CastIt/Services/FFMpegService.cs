using CastIt.Common.Utils;
using CastIt.Interfaces;
using EmbedIO;
using MvvmCross.Logging;
using System;
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
                _logger.Warn($"{nameof(GetFileDuration)}: Couldnt retrieve duration for file = {mrl}. It is not a local / url file");
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

        public Task TranscodeVideo(Stream outputStream, string filePath, double seconds, CancellationToken token)
        {
            string cmd = $@"-v quiet -ss {seconds} -y -i ""{filePath}"" -crf 22 -preset veryfast -c:v libx264 -acodec aac -b:a 128k -movflags frag_keyframe+faststart -f mp4 -";
            _logger.Info($"{nameof(TranscodeVideo)}: Trying to transcode file = {filePath} with CMD = {cmd}");
            _transcodeProcess.StartInfo.Arguments = cmd;
            _transcodeProcess.Start();

            var stream = _transcodeProcess.StandardOutput.BaseStream as FileStream;
            return stream.CopyToAsync(outputStream, token);
        }

        public async Task TranscodeMusic(IHttpContext context, string filePath, double seconds, CancellationToken token)
        {
            //To generate an aac output you need to set adts
            string cmd = $@"-v quiet -ss {seconds} -y -i ""{filePath}"" -preset veryfast -acodec aac -b:a 128k -f adts -";
            _logger.Info($"{nameof(TranscodeMusic)}: Trying to transcode file = {filePath} with CMD = {cmd}");
            _transcodeProcess.StartInfo.Arguments = cmd;
            _transcodeProcess.Start();
            using var memoryStream = new MemoryStream();
            var stream = _transcodeProcess.StandardOutput.BaseStream as FileStream;
            await stream.CopyToAsync(memoryStream, token);
            memoryStream.Position = 0;

            context.Response.ContentLength64 = memoryStream.Length;
            await memoryStream.CopyToAsync(context.Response.OutputStream, token);
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
            return null;
        }
    }
}
