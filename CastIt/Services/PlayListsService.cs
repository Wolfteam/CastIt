using AutoMapper;
using CastIt.Interfaces;
using CastIt.Models;
using CastIt.Models.Entities;
using CastIt.ViewModels.Items;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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
                .Select(p => new PlayList
                {
                    CreatedAt = p.CreatedAt,
                    Id = p.Id,
                    Items = p.Items.OrderBy(f => f.Position),
                    Name = p.Name,
                    Position = p.Position,
                    UpdatedAt = p.UpdatedAt
                })
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<List<PlayListItemViewModel>>(playLists);
        }

        public async Task<PlayListItemViewModel> AddNewPlayList(string name, int position)
        {
            var playlist = new PlayList
            {
                Name = name,
                Position = position,
            };

            try
            {
                _dbContext.Add(playlist);

                await _dbContext.SaveChangesAsync();

                return _mapper.Map<PlayListItemViewModel>(playlist);
            }
            finally
            {
                _dbContext.Entry(playlist).State = EntityState.Detached;
            }
        }

        public async Task UpdatePlayList(long id, string name, int position)
        {
            var playlist = _dbContext.PlayLists.First(pl => pl.Id == id);

            try
            {
                playlist.Name = name;
                playlist.Position = position;

                await _dbContext.SaveChangesAsync();
            }
            finally
            {
                _dbContext.Entry(playlist).State = EntityState.Detached;
            }
        }

        public async Task DeletePlayList(long id)
        {
            var playlist = _dbContext.PlayLists.First(pl => pl.Id == id);
            _dbContext.PlayLists.Remove(playlist);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeletePlayLists(List<long> ids)
        {
            if (!ids.Any())
                return;
            var playlists = await _dbContext.PlayLists.Where(pl => ids.Contains(pl.Id)).ToListAsync();
            _dbContext.PlayLists.RemoveRange(playlists);
            await _dbContext.SaveChangesAsync();
        }

        public Task<FileItemViewModel> AddFile(long playListId, string path, int position)
        {
            var file = new FileItem
            {
                Path = path,
                PlayListId = playListId,
                Position = position,
            };
            return AddFile(file);
        }

        public async Task<FileItemViewModel> AddFile(FileItem file)
        {
            try
            {
                _dbContext.Files.Add(file);
                await _dbContext.SaveChangesAsync();
                return _mapper.Map<FileItemViewModel>(file);
            }
            finally
            {
                _dbContext.Entry(file).State = EntityState.Detached;
            }
        }

        public async Task<List<FileItemViewModel>> AddFiles(List<FileItem> files)
        {
            try
            {
                _dbContext.Files.AddRange(files);
                await _dbContext.SaveChangesAsync();
                return _mapper.Map<List<FileItemViewModel>>(files);
            }
            finally
            {
                foreach (var file in files)
                {
                    _dbContext.Entry(file).State = EntityState.Detached;
                }
            }
        }

        public async Task DeleteFile(long id)
        {
            var file = _dbContext.Files.First(pl => pl.Id == id);
            _dbContext.Files.Remove(file);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteFiles(List<long> ids)
        {
            if (!ids.Any())
                return;
            var files = await _dbContext.Files.Where(pl => ids.Contains(pl.Id)).ToListAsync();
            _dbContext.Files.RemoveRange(files);
            await _dbContext.SaveChangesAsync();
        }

        public void SaveChangesBeforeClosingApp(
            Dictionary<long, int> playListsPositions,
            List<FileItemViewModel> vms)
        {
            SavePlayListsPositions(playListsPositions);
            SaveFileChanges(vms);
            _dbContext.SaveChanges();
        }

        private void SavePlayListsPositions(Dictionary<long, int> positions)
        {
            if (positions.Count == 0)
                return;
            var entities = _dbContext.PlayLists.ToList();
            foreach (var kvp in positions)
            {
                var playlist = entities.First(pl => pl.Id == kvp.Key);
                playlist.Position = kvp.Value;
            }
        }

        private void SaveFileChanges(List<FileItemViewModel> vms)
        {
            if (vms.Count == 0)
                return;
            var ids = vms.Select(f => f.Id).ToList();
            var entities = _dbContext.Files
                .Where(f => ids.Contains(f.Id))
                .ToList();
            foreach (var vm in vms)
            {
                var file = entities.First(f => f.Id == vm.Id);
                file.PlayedPercentage = vm.PlayedPercentage;
                file.Position = vm.Position;
            }
        }
    }
}
