using CastIt.ViewModels.Items;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CastIt.Interfaces
{
    public interface IPlayListsService
    {
        Task<List<PlayListItemViewModel>> GetAllPlayLists();
    }
}