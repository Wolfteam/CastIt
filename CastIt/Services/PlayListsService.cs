using AutoMapper;
using CastIt.Interfaces;
using CastIt.Models;
using CastIt.ViewModels.Items;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CastIt.Services
{
    public class PlayListsService : IPlayListsService
    {
        private readonly IMapper _mapper;
        private readonly AppDbContext _dbContext;

        public PlayListsService(IMapper mapper, AppDbContext dbContext)
        {
            _mapper = mapper;
            _dbContext = dbContext;
        }

        public async Task<List<PlayListItemViewModel>> GetAllPlayLists()
        {
            var playLists = await _dbContext.PlayLists
                .Include(p => p.Items)
                .ToListAsync();

            return _mapper.Map<List<PlayListItemViewModel>>(playLists);
        }
    }
}
