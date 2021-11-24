using CastIt.Application.Common;
using CastIt.Application.Common.Extensions;
using CastIt.Application.Interfaces;
using CastIt.Application.Server;
using CastIt.Domain.Dtos.Responses;
using CastIt.Domain.Entities;
using CastIt.Domain.Enums;
using CastIt.Domain.Extensions;
using CastIt.Domain.Models.FFmpeg.Info;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CastIt.Infrastructure.Models
{
    public class ServerFileItem
    {
        private readonly IFileService _fileService;
        private string _fileName;
        private string _size;
        private string _extension;
        private string _resolution;

        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double TotalSeconds { get; set; }
        public string Path { get; set; }
        public int Position { get; set; }
        public double PlayedPercentage { get; set; }
        public long PlayListId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string Duration { get; private set; }
        public bool PositionChanged { get; set; }
        public bool Loop { get; set; }
        public bool IsBeingPlayed { get; set; }
        public AppFileType Type { get; private set; }
        public bool IsLocalFile
            => Type.IsLocal();
        public bool IsUrlFile
            => Type.IsUrl();
        public bool IsLocalVideoOrMusic
            => Type.IsVideoOrMusic();

        public double PlayedSeconds { get; private set; }

        public bool CanStartPlayingFromCurrentPercentage
            => PlayedPercentage > 0 && PlayedPercentage < 100;
        public bool WasPlayed { get; private set; }
        public bool IsCached
            => !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Description) && !string.IsNullOrWhiteSpace(Path);

        public bool Exists
            => _fileService.Exists(Path);
        public string Filename
        {
            get => _fileName ??= IsCached
                ? Name
                : IsLocalFile
                    ? _fileService.GetFileName(Path)
                    : !string.IsNullOrEmpty(Name)
                        ? Name
                        : Path;
            set => _fileName = value;
        }

        public string Size
            => _size ??= _fileService.GetFileSizeString(Path);
        public string Extension
            => _extension ??= _fileService.GetExtension(Path);
        public string Resolution
            => _resolution ??= !IsLocalFile
                ? string.Empty
                : FileInfo?.GetVideoResolution();
        public string SubTitle
            => IsCached ? Description : Extension.AppendDelimiter("|", Size, Resolution);

        public string PlayedTime
            => FileFormatConstants.FormatDuration(PlayedSeconds);

        public string TotalDuration
            => $"{PlayedTime} / {Duration}";

        public string FullTotalDuration
            => FileFormatConstants.FormatDuration(PlayedSeconds, TotalSeconds, IsUrlFile, Exists);

        public FFProbeFileInfo FileInfo { get; private set; }

        public List<FileItemOptionsResponseDto> CurrentFileVideos { get; }
            = new List<FileItemOptionsResponseDto>();
        public List<FileItemOptionsResponseDto> CurrentFileAudios { get; }
            = new List<FileItemOptionsResponseDto>();
        public List<FileItemOptionsResponseDto> CurrentFileSubTitles { get; }
            = new List<FileItemOptionsResponseDto>();
        public List<FileItemOptionsResponseDto> CurrentFileQualities { get; }
            = new List<FileItemOptionsResponseDto>();

        public int CurrentFileVideoStreamIndex
            => CurrentFileVideos.Find(f => f.IsSelected)?.Id ?? AppWebServerConstants.DefaultSelectedStreamId;
        public int CurrentFileAudioStreamIndex
            => CurrentFileAudios.Find(f => f.IsSelected)?.Id ?? AppWebServerConstants.DefaultSelectedStreamId;
        public int CurrentFileSubTitleStreamIndex
            => CurrentFileSubTitles.Find(f => f.IsSelected)?.Id ?? AppWebServerConstants.NoStreamSelectedId;
        public int CurrentFileQuality
            => CurrentFileQualities.Find(f => f.IsSelected)?.Id ?? AppWebServerConstants.DefaultQualitySelected;

        private ServerFileItem(IFileService fileService)
        {
            _fileService = fileService;
        }

        public static ServerFileItem From(IFileService fileService, FileItem file)
            => From(fileService, file, null);

        public static ServerFileItem From(IFileService fileService, FileItem file, FFProbeFileInfo fileInfo)
        {
            var serverFileItem = new ServerFileItem(fileService)
            {
                Id = file.Id,
                Name = file.Name,
                Description = file.Description,
                TotalSeconds = file.TotalSeconds,
                Path = file.Path,
                Position = file.Position,
                PlayedPercentage = file.PlayedPercentage,
                CreatedAt = file.CreatedAt,
                UpdatedAt = file.UpdatedAt,
                PlayListId = file.PlayListId,
                Type = fileService.GetFileType(file.Path),
                PlayedSeconds = file.PlayedPercentage * file.TotalSeconds / 100
            };

            return serverFileItem.UpdateFileInfo(fileInfo);
        }

        public ServerFileItem UpdateFileInfo(FFProbeFileInfo fileInfo, double totalSeconds)
        {
            TotalSeconds = totalSeconds;
            return UpdateFileInfo(fileInfo);
        }

        public ServerFileItem UpdateFileInfo(FFProbeFileInfo fileInfo)
        {
            FileInfo = fileInfo;
            if (!Exists)
            {
                TotalSeconds = 0;
                Duration = AppWebServerConstants.MissingFileText;
                return this;
            }

            var totalSeconds = GetTotalSecondsToUse(fileInfo);
            TotalSeconds = totalSeconds;
            if (totalSeconds <= 0)
            {
                Duration = "N/A";
                return this;
            }
            Duration = FileFormatConstants.FormatDuration(totalSeconds);
            return this;
        }

        public ServerFileItem BeingPlayed(bool played = true)
        {
            IsBeingPlayed = played;
            if (IsBeingPlayed)
                WasPlayed = true;
            return this;
        }

        public ServerFileItem UpdatePlayedPercentage(double newValue)
        {
            PlayedPercentage = newValue;
            return this;
        }

        public ServerFileItem UpdatePlayedSeconds(double newValue)
        {
            PlayedSeconds = newValue;
            return this;
        }

        public ServerFileItem EndReached()
        {
            PlayedPercentage = 100;
            return BeingPlayed(false);
        }

        public ServerFileItem CleanAllStreams()
        {
            CurrentFileVideos.Clear();
            CurrentFileAudios.Clear();
            CurrentFileQualities.Clear();
            CurrentFileSubTitles.Clear();
            return this;
        }

        public ServerFileItem SetVideoStreams()
        {
            CheckBeforeSettingStream();
            CurrentFileVideos.Clear();
            bool isSelected = true;
            bool isEnabled = FileInfo.Videos.Count > 1;
            foreach (var video in FileInfo.Videos)
            {
                CurrentFileVideos.Add(FileItemOptionsResponseDto.ForVideo(video.Index, isSelected, isEnabled, video.VideoText));
                isSelected = false;
            }

            return this;
        }

        public ServerFileItem SetAudioStreams()
        {
            CheckBeforeSettingStream();
            CurrentFileAudios.Clear();
            bool isSelected = true;
            bool isEnabled = FileInfo.Audios.Count > 1;
            foreach (var audio in FileInfo.Audios)
            {
                CurrentFileAudios.Add(FileItemOptionsResponseDto.ForAudio(audio.Index, isSelected, isEnabled, audio.AudioText));
                isSelected = false;
            }

            return this;
        }

        public ServerFileItem SetSubtitleStreams(string localSubsPath, string filename, bool loadFirstSubtitleFoundAutomatically)
        {
            CheckBeforeSettingStream();
            CurrentFileSubTitles.Clear();
            if (!Type.IsLocalVideo())
                return this;

            bool localSubExists = !string.IsNullOrEmpty(localSubsPath);
            bool isEnabled = FileInfo.SubTitles.Count > 1 || localSubExists;
            //TODO: TRANSLATE THE NONE ?
            CurrentFileSubTitles.Add(FileItemOptionsResponseDto.ForEmbeddedSubtitles(
                AppWebServerConstants.NoStreamSelectedId,
                !localSubExists && FileInfo.SubTitles.Count == 0 || !loadFirstSubtitleFoundAutomatically,
                isEnabled,
                "None"));
            if (localSubExists)
            {
                CurrentFileSubTitles.Add(FileItemOptionsResponseDto.ForLocalSubtitles(AppWebServerConstants.NoStreamSelectedId - 1, filename, localSubsPath));
            }

            isEnabled = FileInfo.SubTitles.Count > 0;
            bool isSelected = !localSubExists && loadFirstSubtitleFoundAutomatically;
            foreach (var subtitle in FileInfo.SubTitles)
            {
                CurrentFileSubTitles.Add(FileItemOptionsResponseDto.ForEmbeddedSubtitles(subtitle.Index, isSelected, isEnabled, subtitle.SubTitleText));
                isSelected = false;
            }

            return this;
        }

        public ServerFileItem SetQualitiesStreams(int selectedQuality, List<int> qualities)
        {
            CurrentFileQualities.Clear();
            CurrentFileQualities.AddRange(qualities.Select(q => FileItemOptionsResponseDto.ForQuality(q, q == selectedQuality)));
            return this;
        }

        public FileItemOptionsResponseDto GetAudioFileOption(int streamIndex)
            => GetFileOption(streamIndex, true, false);

        public FileItemOptionsResponseDto GetSubsFileOption(int streamIndex)
            => GetFileOption(streamIndex, false, true);

        public FileItemOptionsResponseDto GetQualityFileOption(int streamIndex)
            => GetFileOption(streamIndex, false, false);

        public FileItemOptionsResponseDto GetFileOption(int streamIndex, bool isAudio, bool isSubTitle)
        {
            var options = isAudio
                ? CurrentFileAudios.Find(a => a.Id == streamIndex)
                : isSubTitle
                    ? CurrentFileSubTitles.Find(s => s.Id == streamIndex)
                    : CurrentFileQualities.Find(q => q.Id == streamIndex);

            return options;
        }

        public ServerFileItem SetSelectedFileOption(FileItemOptionsResponseDto selectedItem)
        {
            if (selectedItem == null)
            {
                throw new NullReferenceException("The selected file option is null");
            }
            var options = selectedItem.IsVideo
                ? CurrentFileVideos
                : selectedItem.IsAudio
                    ? CurrentFileAudios
                    : selectedItem.IsSubTitle
                        ? CurrentFileSubTitles
                        : selectedItem.IsQuality
                            ? CurrentFileQualities
                            : throw new ArgumentOutOfRangeException(
                                "File option changed, but the one that changes is not audio, " +
                                "nor video, nor subs, nor quality");

            foreach (var item in options.Where(item => item.IsSelected))
            {
                item.IsSelected = false;
            }

            selectedItem.IsSelected = true;
            return this;
        }

        private void CheckBeforeSettingStream()
        {
            if (FileInfo == null)
                throw new InvalidOperationException("The file info cannot be null while attempting to set streams");
        }

        private double GetTotalSecondsToUse(FFProbeFileInfo fileInfo)
        {
            //The file is already cached / loaded
            if (TotalSeconds > 0 && fileInfo == null)
            {
                return TotalSeconds;
            }

            //A url file won't have a file info (unless it is an hls)
            if (IsUrlFile && TotalSeconds > 0 && fileInfo?.Format?.Duration == 0)
            {
                return TotalSeconds;
            }

            //Either the file info was loaded or haven't loaded it yet
            return fileInfo?.Format?.Duration ?? -1;
        }
    }
}
