using AutoMapper;
using CastIt.Common.Utils;
using CastIt.Interfaces;
using CastIt.Models.Entities;
using CastIt.ViewModels.Items;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CastIt.Services
{
    //We will use the synchronus calls until this gets fixed
    //https://github.com/praeclarum/sqlite-net/issues/881#issuecomment-569182348
    public class AppDataService : IPlayListsService
    {
        private readonly IMapper _mapper;
        private readonly string _connectionString;

        public AppDataService(IMapper mapper)
        {
            _mapper = mapper;

            _connectionString = FileUtils.GetDbConnectionString();

            CreateDb();
        }

        #region Methods
        public async Task<FileItemViewModel> AddFile(
            long playListId,
            string path,
            int position,
            string name = null)
        {
            using var db = new SQLiteConnection(_connectionString);
            var file = new FileItem
            {
                CreatedAt = DateTime.Now,
                Path = path,
                PlayListId = playListId,
                Position = position,
                Name = name
            };

            db.Insert(file);

            return _mapper.Map<FileItemViewModel>(file);
        }

        public async Task<List<FileItemViewModel>> AddFiles(List<FileItem> files)
        {
            using var db = new SQLiteConnection(_connectionString);
            db.InsertAll(files);

            return _mapper.Map<List<FileItemViewModel>>(files);
        }

        public async Task<PlayListItemViewModel> AddNewPlayList(string name, int position)
        {
            var playlist = new PlayList
            {
                CreatedAt = DateTime.Now,
                Name = name,
                Position = position,
            };
            using var db = new SQLiteConnection(_connectionString);
            db.Insert(playlist);
            return _mapper.Map<PlayListItemViewModel>(playlist);
        }

        public async Task DeleteFile(long id)
        {
            using var db = new SQLiteConnection(_connectionString);
            db.Delete<FileItem>(id);
        }

        public async Task DeleteFiles(List<long> ids)
        {
            if (ids.Count == 0)
                return;
            using var db = new SQLiteConnection(_connectionString);
            var files = db.Table<FileItem>()
                .Where(f => ids.Contains(f.Id))
                .ToList();
            foreach (var file in files)
            {
                db.Delete(file);
            }
        }

        public async Task DeletePlayList(long id)
        {
            using var db = new SQLiteConnection(_connectionString);
            db.Delete<PlayList>(id);
        }

        public async Task DeletePlayLists(List<long> ids)
        {
            if (ids.Count == 0)
                return;
            using var db = new SQLiteConnection(_connectionString);
            var playlists = db.Table<PlayList>()
                .Where(f => ids.Contains(f.Id))
                .ToList();
            foreach (var playlist in playlists)
            {
                db.Delete(playlist);
            }
        }

        public async Task<List<PlayListItemViewModel>> GetAllPlayLists()
        {
            using var db = new SQLiteConnection(_connectionString);
            var playlists = db.Table<PlayList>().ToList();
            return _mapper.Map<List<PlayListItemViewModel>>(playlists);
        }

        public async Task<List<FileItemViewModel>> GetAllFiles(long playlistId)
        {
            using var db = new SQLiteConnection(_connectionString);
            var files = db.Table<FileItem>().Where(f => f.PlayListId == playlistId).ToList();
            return _mapper.Map<List<FileItemViewModel>>(files);
        }

        public void SaveChangesBeforeClosingApp(Dictionary<PlayListItemViewModel, int> playListsPositions, List<FileItemViewModel> vms)
        {
            SavePlayListsPositions(playListsPositions);
            SaveFileChanges(vms);
        }

        public async Task UpdatePlayList(long id, string name, int position)
        {
            using var db = new SQLiteConnection(_connectionString);
            var playlist = db.Table<PlayList>().Where(pl => pl.Id == id).First();
            playlist.Name = name;
            playlist.Position = position;
            db.Update(playlist);
        }

        private void CreateDb()
        {
            bool dbExists = File.Exists(_connectionString);
            using var db = new SQLiteConnection(_connectionString);
            db.CreateTable<PlayList>();
            db.CreateTable<FileItem>();
            if (dbExists)
                return;
            var playList = new PlayList
            {
                CreatedAt = DateTime.Now,
                Name = "Default",
                Position = 1,
            };
            db.Insert(playList);
        }

        private void SavePlayListsPositions(Dictionary<PlayListItemViewModel, int> positions)
        {
            if (positions.Count == 0)
                return;
            using var db = new SQLiteConnection(_connectionString);
            var playlists = db.Table<PlayList>().ToList();
            foreach (var kvp in positions)
            {
                var vm = kvp.Key;
                var playlist = playlists.First(pl => pl.Id == vm.Id);
                playlist.Position = kvp.Value;
                playlist.Shuffle = vm.Shuffle;
                playlist.Loop = vm.Loop;
            }

            db.UpdateAll(playlists);
        }

        private void SaveFileChanges(List<FileItemViewModel> vms)
        {
            if (vms.Count == 0)
                return;
            var ids = vms.Select(f => f.Id).ToList();
            using var db = new SQLiteConnection(_connectionString);
            var entities = db.Table<FileItem>()
                .Where(f => ids.Contains(f.Id))
                .ToList();
            foreach (var vm in vms)
            {
                var file = entities.First(f => f.Id == vm.Id);
                file.PlayedPercentage = vm.PlayedPercentage;
                file.Position = vm.Position;
            }

            db.UpdateAll(entities);
        }
        #endregion
    }
}
