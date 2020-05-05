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
            //var playlists = new List<PlayList>
            //{
            //    new PlayList
            //    {
            //        Id = 1,
            //        Name = "algo",
            //        Items = new List<FileItem>
            //        {
            //            new FileItem
            //            {
            //                Id = 1,
            //                Path = "C:\\Users\\Efrain Bastidas\\Music\\B Gata H Kei  Nonononon.mp3",
            //                PlayListId = 1,
            //                Position= 1,
            //            },
            //            new FileItem
            //            {
            //                Id = 1,
            //                Path = "C:\\Users\\Efrain Bastidas\\Music\\Nanahira  課金厨のうた -More Charin Ver.-.mp3",
            //                PlayListId = 1,
            //                Position= 2,
            //            }
            //        }
            //    },
            //    new PlayList
            //    {
            //        Id = 2,
            //        Name = "algo",
            //        Items = new List<FileItem>
            //        {
            //            new FileItem
            //            {
            //                Id = 1,
            //                Path = "C:\\Users\\Efrain Bastidas\\Music\\B Gata H Kei  Nonononon.mp3",
            //                PlayListId = 2,
            //                Position= 1,
            //            },
            //            new FileItem
            //            {
            //                Id = 1,
            //                Path = "C:\\Users\\Efrain Bastidas\\Music\\Nanahira  課金厨のうた -More Charin Ver.-.mp3",
            //                PlayListId = 2,
            //                Position= 2,
            //            }
            //        }
            //    }
            //};

            var playLists = await _dbContext.PlayLists
                .Include(p => p.Items)
                .Select(p => new PlayList
                {
                    CreatedAt = p.CreatedAt,
                    Id = p.Id,
                    Items = p.Items.OrderBy(f => f.Position).Select(f => f),
                    Name = p.Name,
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

        //TODO: REMOVE THIS METHOD
        public async Task SavePlayLists(List<PlayListItemViewModel> playLists)
        {
            return;
            //try
            //{
            //    var toSave = playLists.Select(pl => new PlayList
            //    {
            //        Name = pl.Name,
            //        Items = pl.Items.Select(f => new FileItem
            //        {
            //            Path = f.Path,
            //            Position = f.Position,
            //        }).ToList()
            //    });
            //    var entitiesToDelete = await _dbContext.PlayLists.ToListAsync();
            //    _dbContext.RemoveRange(entitiesToDelete);

            //    _dbContext.AddRange(toSave);
            //    await _dbContext.SaveChangesAsync();
            //}
            //catch (Exception e)
            //{
            //    System.Diagnostics.Debug.WriteLine(e);
            //}
        }
    }
}
