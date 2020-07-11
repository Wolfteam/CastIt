using CastIt.Common.Enums;
using CastIt.Server.Dtos.Responses;
using MvvmCross.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CastIt.Interfaces.ViewModels
{
    public interface IMainViewModel : IBaseViewModel
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
        bool IsExpanded { get; set; }
        bool IsMuted { get; set; }
        bool IsPaused { get; set; }
        double PlayedPercentage { get; set; }
        string PreviewThumbnailImg { get; set; }
        int SelectedPlayListIndex { get; set; }
        double VolumeLevel { get; set; }
        IMvxAsyncCommand CloseAppCommand { get; }
        IMvxAsyncCommand<long> GoToSecondsCommand { get; }
        IMvxCommand NextCommand { get; }
        IMvxCommand PreviousCommand { get; }
        IMvxAsyncCommand SetVolumeCommand { get; }
        IMvxAsyncCommand<int> SkipCommand { get; }
        IMvxAsyncCommand StopPlayBackCommand { get; }
        IMvxCommand SwitchPlayListsCommand { get; }
        IMvxAsyncCommand ToggleMuteCommand { get; }
        IMvxAsyncCommand TogglePlayBackCommand { get; }

        List<GetAllPlayListResponseDto> GetAllPlayLists();
        PlayListItemResponseDto GetPlayList(long playlistId);
        FileLoadedResponseDto GetCurrentFileLoaded();
        Task PlayFile(long id, long playlistId);
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
    }
}