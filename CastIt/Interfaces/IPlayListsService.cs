using CastIt.Models.Entities;
using CastIt.ViewModels.Items;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CastIt.Interfaces
{
    public interface IPlayListsService
    {
        Task<List<PlayListItemViewModel>> GetAllPlayLists();
        Task<PlayListItemViewModel> AddNewPlayList(string name, int position);
        Task UpdatePlayList(long id, string name, int position);
        Task DeletePlayList(long id);
        Task DeletePlayLists(List<long> ids);

        Task<List<FileItemViewModel>> GetAllFiles(long playlistId);
        Task<FileItemViewModel> AddFile(long playListId, string path, int position);
        Task<List<FileItemViewModel>> AddFiles(List<FileItem> files);
        Task DeleteFile(long id);
        Task DeleteFiles(List<long> ids);

        void SaveChangesBeforeClosingApp(Dictionary<long, int> playListsPositions, List<FileItemViewModel> vms);
    }
}