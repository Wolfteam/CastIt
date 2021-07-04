using CastIt.Domain.Dtos.Responses;
using CastIt.ViewModels.Items;
using System.Collections.Generic;
using System.Linq;

namespace CastIt.ViewModels
{
    public partial class MainViewModel
    {
        private void CastItHubOnOnClientConnected()
        {
            ServerIsRunning = true;
        }

        private void CastItHubOnOnClientDisconnected()
        {
            ServerIsRunning = false;
            OnStoppedPlayBack();
            PlayLists.Clear();
            GoBackCommand.Execute();
        }

        private void OnPlayerStatusChanged(ServerPlayerStatusResponseDto status)
        {
            if (_updatingPlayerStatus)
            {
                return;
            }

            _updatingPlayerStatus = true;
            IsPaused = status.Player.IsPaused;
            VolumeLevel = status.Player.VolumeLevel;
            IsMuted = status.Player.IsMuted;
            CurrentFileThumbnail = status.PlayedFile?.ThumbnailUrl;
            CurrentFileDuration = status.PlayedFile?.TotalSeconds ?? 1; //Has to be one, in order for the slider to show correctly;
            var playedFile = status.PlayedFile;
            if (playedFile != null)
            {
                UpdatedCommonPlayedStuff(status.Player.IsPlayingOrPaused, playedFile);
                UpdatePlayListSpecificStuff(status, playedFile);
            }
            else
            {
                OnStoppedPlayBack();
            }

            _updatingPlayerStatus = false;
        }

        private void OnPlayListsLoaded(List<GetAllPlayListResponseDto> playLists)
        {
            PlayLists.Clear();
            var mapped = playLists.ConvertAll(pl => PlayListItemViewModel.From(pl, _mapper));
            PlayLists.ReplaceWith(mapped);
            IsBusy = false;
        }

        private void OnPlayListAdded(GetAllPlayListResponseDto playList)
        {
            var vm = _mapper.Map<PlayListItemViewModel>(playList);
            PlayLists.Add(vm);
            SelectedPlayListIndex = PlayLists.Count - 1;
        }

        private void OnPlayListsChanged(List<GetAllPlayListResponseDto> playLists)
        {
            foreach (var playList in playLists)
            {
                OnPlayListChanged(playList);
            }
        }

        private void OnPlayListChanged(GetAllPlayListResponseDto playList)
        {
            var vm = PlayLists.FirstOrDefault(f => f.Id == playList.Id);
            if (vm == null)
            {
                return;
            }

            vm.IsBusy = true;
            if (vm.Position != playList.Position)
            {
                var currentIndex = PlayLists.IndexOf(vm);
                //if the indexes are equal, the move method will throw an exception
                if (currentIndex != playList.Position)
                    PlayLists.Move(currentIndex, playList.Position);
            }
            vm.IsBusy = false;
            _mapper.Map(playList, vm);
        }

        private void OnPlayListDeleted(long id)
        {
            var playList = PlayLists.FirstOrDefault(pl => pl.Id == id);
            if (playList != null)
            {
                PlayLists.Remove(playList);
            }
        }

        private void OnPlayListBusy(long id, bool isBusy)
        {
            var playList = PlayLists.FirstOrDefault(pl => pl.Id == id);
            if (playList != null)
            {
                playList.IsBusy = isBusy;
            }
        }

        private void OnFileAdded(FileItemResponseDto file)
        {
            //TODO: IF THE ITEMS ARE NOT LOADED THIS WON'T WORK
            var playList = PlayLists.FirstOrDefault(pl => pl.Id == file.PlayListId);
            if (playList != null)
            {
                var vm = _mapper.Map<FileItemViewModel>(file);
                playList.Items.Insert(file.Position - 1, vm);
            }
        }

        private void OnFilesChanged(List<FileItemResponseDto> files)
        {
            foreach (var file in files)
            {
                OnFileChanged(file);
            }
        }

        private void OnFileChanged(FileItemResponseDto file)
        {
            var playList = PlayLists.FirstOrDefault(pl => pl.Id == file.PlayListId);
            var vm = playList?.Items.FirstOrDefault(f => f.Id == file.Id);
            if (vm == null)
                return;
            if (file.Position != vm.Position)
            {
                var currentIndex = playList.Items.IndexOf(vm);
                playList.Items.Move(currentIndex, file.Position - 1);
            }
            _mapper.Map(file, vm);
        }

        private void OnFileDeleted(long playListId, long id)
        {
            var playList = PlayLists.FirstOrDefault(pl => pl.Id == playListId);
            var vm = playList?.Items.FirstOrDefault(f => f.Id == id);
            if (vm != null)
            {
                playList.Items.Remove(vm);
            }
        }

        private void OnFileLoaded(FileItemResponseDto playedFile)
        {
            IsBusy = false;
            var playList = PlayLists.FirstOrDefault(pl => pl.Id == playedFile.PlayListId);

            if (CurrentPlayedFile != null && playList != null)
            {
                playList.SelectedItem = CurrentPlayedFile;
            }
        }

        private void OnFileLoading(FileItemResponseDto file)
        {
            IsBusy = true;
            SetCurrentlyPlayingInfo(file.Filename, false, file.PlayedPercentage, file.PlayedSeconds);
        }

        private void OnFileEndReached(FileItemResponseDto file)
        {
            IsBusy = false;
            IsPaused = false;
            CurrentPlayedFile?.OnEndReached();
            SetCurrentlyPlayingInfo(null, false);
        }

        private void OnStoppedPlayBack()
        {
            CurrentPlayedFile?.OnStopped();
            CurrentPlayedFile = null;
            IsBusy = false;
            IsPaused = false;
            SetCurrentlyPlayingInfo(null, false);
        }

        private void UpdatedCommonPlayedStuff(bool isPlayingOrPaused, FileItemResponseDto playedFile)
        {
            if (playedFile.CurrentFileVideos.Any())
            {
                UpdateFileOptionsIfNeeded(CurrentFileVideos, playedFile.CurrentFileVideos);
            }

            if (playedFile.CurrentFileAudios.Any())
            {
                UpdateFileOptionsIfNeeded(CurrentFileAudios, playedFile.CurrentFileAudios);
            }

            if (playedFile.CurrentFileQualities.Any())
            {
                UpdateFileOptionsIfNeeded(CurrentFileQualities, playedFile.CurrentFileQualities);
            }

            if (playedFile.CurrentFileSubTitles.Any())
            {
                UpdateFileOptionsIfNeeded(CurrentFileSubTitles, playedFile.CurrentFileSubTitles);
            }

            ElapsedTimeString = playedFile.FullTotalDuration;
            SetCurrentlyPlayingInfo(playedFile.Filename, isPlayingOrPaused, playedFile.PlayedPercentage, playedFile.PlayedSeconds);
        }

        private void UpdatePlayListSpecificStuff(ServerPlayerStatusResponseDto status, FileItemResponseDto playedFile)
        {
            if (status == null || playedFile == null)
            {
                return;
            }
            //This may happen when you open the app and something was being played
            bool differentPlayedFile = playedFile.Id != CurrentPlayedFile?.Id;
            if (differentPlayedFile)
                CurrentPlayedFile?.OnStopped();

            if (!_thumbnailRanges.Any() || differentPlayedFile)
            {
                _thumbnailRanges = status.ThumbnailRanges;
            }

            var playlist = PlayLists.FirstOrDefault(pl => pl.Id == playedFile.PlayListId);
            //Do not update the playlist if it hasn't been loaded
            if (playlist?.Loading == true)
            {
                _updatingPlayerStatus = false;
                return;
            }

            CurrentPlayedFile = playlist?.Items.FirstOrDefault(pl => pl.Id == playedFile.Id);

            //The playlist hasn't been loaded, so we add the file item in order to have preview thumbnails
            if (CurrentPlayedFile == null)
            {
                CurrentPlayedFile = _mapper.Map<FileItemViewModel>(playedFile);
                playlist?.Items.Add(CurrentPlayedFile);
            }

            if (playedFile.Id == CurrentPlayedFile.Id)
            {
                CurrentPlayedFile.OnChange(playedFile);
            }

            if (playlist == null)
                return;
            playlist.ImageUrl = CurrentFileThumbnail;
            playlist.PlayedTime = playlist.PlayedTime;
            playlist.TotalDuration = status.PlayList.TotalDuration;
            if (CurrentPlayedFile != null && playlist.SelectedItem == null && playlist.SelectedItem != CurrentPlayedFile)
            {
                playlist.SelectedItem = CurrentPlayedFile;
            }

            //make sure we don't have nothing being played in this playlist except for the CurrentPlayedFile
            foreach (var notPlayedFile in playlist.Items.Where(f => f.IsBeingPlayed && f.Id != CurrentPlayedFile?.Id).ToList())
            {
                notPlayedFile.OnStopped();
            }
        }
    }
}
