using CastIt.Domain.Entities;
using CastIt.ViewModels.Items;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CastIt.Interfaces
{
    public interface IAppDataService
    {
        void Close();
        Task<List<PlayListItemViewModel>> GetAllPlayLists();
        Task<PlayListItemViewModel> AddNewPlayList(string name, int position);
        Task UpdatePlayList(long id, string name, int position);
        Task DeletePlayList(long id);
        Task DeletePlayLists(List<long> ids);

        Task<List<FileItemViewModel>> GetAllFiles(long playlistId);
        Task<FileItemViewModel> AddFile(
            long playListId,
            string path,
            int position,
            string name = null,
            string description = null,
            double duration = 0);
        Task<List<FileItemViewModel>> AddFiles(List<FileItem> files);
        Task DeleteFile(long id);
        Task DeleteFiles(List<long> ids);
        Task UpdateFile(long id, string name, string description, double duration);

        void SaveChangesBeforeClosingApp(Dictionary<PlayListItemViewModel, int> playListsPositions, List<FileItemViewModel> vms);
    }
}