using CastIt.Domain.Dtos.Responses;
using CastIt.Domain.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CastIt.Server.Interfaces
{
    public interface IViewForMediaWebSocket
    {
        int CurrentFileAudioStreamIndex { get; }
        double CurrentFileDuration { get; }
        int CurrentFileQuality { get; }
        int CurrentFileSubTitleStreamIndex { get; }
        string CurrentFileThumbnail { get; set; }
        int CurrentFileVideoStreamIndex { get; }
        string CurrentlyPlayingFilename { get; set; }
        double CurrentPlayedSeconds { get; set; }
        bool IsBusy { get; set; }
        bool IsCurrentlyPlaying { get; set; }
        bool IsMuted { get; set; }
        bool IsPaused { get; set; }
        double PlayedPercentage { get; set; }
        string PreviewThumbnailImg { get; set; }
        int SelectedPlayListIndex { get; set; }
        double VolumeLevel { get; set; }

        Task CloseAppFromMediaWebSocket();
        Task GoToSecondsFromMediaWebSocket(long seconds);
        void GoToNextFromMediaWebSocket();
        void GoToPreviousFromMediaWebSocket();
        Task SetVolumeFromMediaWebSocket();
        Task SkipFromMediaWebSocket(int seconds);
        Task StopPlayBackFromMediaWebSocket();
        Task ToggleMuteFromMediaWebSocket();
        Task TogglePlayBackFromMediaWebSocket();

        List<GetAllPlayListResponseDto> GetAllPlayListsForMediaWebSocket();
        PlayListItemResponseDto GetPlayListForMediaWebSocket(long playlistId);
        FileLoadedResponseDto GetCurrentFileLoadedForMediaWebSocket();
        Task PlayFileForMediaWebSocket(long id, long playlistId, bool force);
        void SetPlayListOptions(long id, bool loop, bool shuffle);
        Task DeletePlayList(long id);
        Task DeleteFile(long id, long playListId);
        Task SetFileLoop(long id, long playlistId, bool loop);
        Task SetFileOptions(int streamIndex, bool isAudio, bool isSubtitle, bool isQuality);
        void UpdateSettings(
            bool startFilesFromTheStart,
            bool playNextFileAutomatically,
            bool forceVideoTranscode,
            bool forceAudioTranscode,
            VideoScaleType videoScale,
            bool enableHardwareAcceleration);
        List<FileItemOptionsResponseDto> GetFileOptions(long id);
        Task RenamePlayList(long id, string newName);
        AppSettingsResponseDto GetCurrentAppSettings();
    }
}
