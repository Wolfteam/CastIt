using CastIt.Application.Common;
using CastIt.Application.Common.Extensions;
using CastIt.Application.Interfaces;
using CastIt.Domain.Models.FFmpeg.Info;

namespace CastIt.Test.Models
{
    public class ServerFileItem
    {
        private readonly IFileService _fileService;
        private string _fileName;

        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double TotalSeconds { get; set; }
        public string Path { get; set; }
        public int Position { get; set; }
        public double PlayedPercentage { get; set; }
        public long PlayListId { get; set; }

        public string Duration { get; set; }
        public bool PositionChanged { get; set; }
        public bool Loop { get; set; }
        public bool IsBeingPlayed { get; set; }
        public bool IsLocalFile
            => _fileService.IsLocalFile(Path);
        public bool IsUrlFile
            => _fileService.IsUrlFile(Path);
        public double PlayedSeconds
            => PlayedPercentage * TotalSeconds / 100;
        public bool CanStartPlayingFromCurrentPercentage
            => PlayedPercentage > 0 && PlayedPercentage < 100;
        public bool WasPlayed
            => PlayedPercentage > 0 && PlayedPercentage <= 100;
        public bool IsCached
            => !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Description) && !string.IsNullOrWhiteSpace(Path);

        public bool Exists
            => IsLocalFile || IsUrlFile;
        public string Filename
        {
            get => _fileName ??= IsCached
                ? Name
                : _fileService.IsLocalFile(Path)
                    ? _fileService.GetFileName(Path)
                    : !string.IsNullOrEmpty(Name)
                        ? Name
                        : Path;
            set => _fileName = value;
        }
        public string Size
            => _fileService.GetFileSizeString(Path);
        public string Extension
            => _fileService.GetExtension(Path);
        public string Resolution
            => !IsLocalFile
                ? string.Empty
                : FileInfo?.GetVideoResolution();
        public string SubTitle
            => IsCached ? Description : Extension.AppendDelimiter("|", Size, Resolution);

        public string PlayedTime
        {
            get
            {
                var formatted = FileFormatConstants.FormatDuration(PlayedSeconds);
                return $"{formatted}";
            }
        }

        public string TotalDuration
            => $"{PlayedTime} / {Duration}";

        public FFProbeFileInfo FileInfo { get; }

        public ServerFileItem(IFileService fileService, FFProbeFileInfo fileInfo)
        {
            _fileService = fileService;
            FileInfo = fileInfo;
        }
    }
}
