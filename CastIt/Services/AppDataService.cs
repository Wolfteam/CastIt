using AutoMapper;
using CastIt.Application.Common.Utils;
using CastIt.Domain.Entities;
using CastIt.Interfaces;
using CastIt.Migrations;
using CastIt.ViewModels.Items;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CastIt.Services
{
    public class AppDataService : IAppDataService
    {
        private readonly IMapper _mapper;
        private readonly string _connectionString;
        private readonly IFreeSql _db;

        public AppDataService(IMapper mapper)
        {
            _mapper = mapper;
            _connectionString = AppFileUtils.GetDbConnectionString();
            _db = new FreeSql.FreeSqlBuilder()
               .UseConnectionString(FreeSql.DataType.Sqlite, _connectionString)
               .UseAutoSyncStructure(false)
               .Build();

            _db.CodeFirst.ConfigEntity<PlayList>(pl => pl.Property(x => x.Id).IsPrimary(true).IsIdentity(true));
            _db.CodeFirst.ConfigEntity<FileItem>(f => f.Property(x => x.Id).IsPrimary(true).IsIdentity(true));
            ApplyMigrations();
        }

        #region Methods

        public void Close()
        {
            _db.Dispose();
        }

        public async Task<FileItemViewModel> AddFile(
            long playListId,
            string path,
            int position,
            string name = null,
            string description = null,
            double duration = 0)
        {
            var file = new FileItem
            {
                CreatedAt = DateTime.Now,
                Path = path,
                PlayListId = playListId,
                Position = position,
                Name = name,
                Description = description,
                TotalSeconds = duration,
            };
            file.Id = await _db.Insert(file).ExecuteIdentityAsync();
            return _mapper.Map<FileItemViewModel>(file);
        }

        public async Task<List<FileItemViewModel>> AddFiles(List<FileItem> files)
        {
            var list = new List<FileItemViewModel>();
            foreach (var file in files)
            {
                file.Id = await _db.Insert(file).ExecuteIdentityAsync();
                var mapped = _mapper.Map<FileItemViewModel>(file);
                list.Add(mapped);
            }

            return list;
        }

        public async Task<PlayListItemViewModel> AddNewPlayList(string name, int position)
        {
            var playlist = new PlayList
            {
                CreatedAt = DateTime.Now,
                Name = name,
                Position = position,
            };
            playlist.Id = await _db.Insert(playlist).ExecuteIdentityAsync();
            return _mapper.Map<PlayListItemViewModel>(playlist);
        }

        public Task DeleteFile(long id)
        {
            return _db.Delete<FileItem>().Where(f => f.Id == id).ExecuteAffrowsAsync();
        }

        public Task DeleteFiles(List<long> ids)
        {
            return ids.Count == 0
                ? Task.CompletedTask
                : _db.Delete<FileItem>().Where(f => ids.Contains(f.Id)).ExecuteAffrowsAsync();
        }

        public async Task DeletePlayList(long id)
        {
            await _db.Delete<FileItem>().Where(f => f.PlayListId == id).ExecuteAffrowsAsync();
            await _db.Delete<PlayList>().Where(p => p.Id == id).ExecuteAffrowsAsync();
        }

        public Task DeletePlayLists(List<long> ids)
        {
            return ids.Count == 0
                ? Task.CompletedTask
                : _db.Delete<PlayList>().Where(p => ids.Contains(p.Id)).ExecuteAffrowsAsync();
        }

        public async Task<List<PlayListItemViewModel>> GetAllPlayLists()
        {
            var playLists = await _db.Select<PlayList>().ToListAsync();
            return _mapper.Map<List<PlayListItemViewModel>>(playLists);
        }

        public async Task<List<FileItemViewModel>> GetAllFiles(long playlistId)
        {
            var files = await _db.Select<FileItem>().Where(pl => pl.PlayListId == playlistId).ToListAsync();
            return _mapper.Map<List<FileItemViewModel>>(files);
        }

        public void SaveChangesBeforeClosingApp(Dictionary<PlayListItemViewModel, int> playListsPositions, List<FileItemViewModel> vms)
        {
            SavePlayListsPositions(playListsPositions);
            SaveFileChanges(vms);
        }

        public Task UpdatePlayList(long id, string name, int position)
        {
            return _db.Update<PlayList>(id)
                .Set(p => p.Name, name)
                .Set(p => p.Position, position)
                .Set(p => p.UpdatedAt, DateTime.Now)
                .ExecuteAffrowsAsync();
        }

        public Task UpdateFile(long id, string name, string description, double duration)
        {
            return _db.Update<FileItem>(id)
                .Set(f => f.Name, name)
                .Set(f => f.Description, description)
                .Set(f => f.TotalSeconds, duration)
                .Set(f => f.UpdatedAt, DateTime.Now)
                .ExecuteAffrowsAsync();
        }

        private void SavePlayListsPositions(Dictionary<PlayListItemViewModel, int> positions)
        {
            if (positions.Count == 0)
                return;

            foreach (var (vm, position) in positions)
            {
                _db.Update<PlayList>(vm.Id)
                    .Set(p => p.Position, position)
                    .Set(p => p.Shuffle, vm.Shuffle)
                    .Set(p => p.Loop, vm.Loop)
                    .ExecuteAffrows();
            }
        }

        private void SaveFileChanges(List<FileItemViewModel> vms)
        {
            if (vms.Count == 0)
                return;

            foreach (var vm in vms)
            {
                _db.Update<FileItem>(vm.Id)
                    .Set(f => f.PlayedPercentage, vm.PlayedPercentage)
                    .Set(f => f.Position, vm.Position)
                    .ExecuteAffrows();
            }
        }

        private void ApplyMigrations()
        {
            var provider = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddSQLite()
                    .WithGlobalConnectionString(_connectionString)
                    .ScanIn(typeof(InitPlayList).Assembly).For.Migrations())
                .AddLogging(lb => lb.AddFluentMigratorConsole())
                .BuildServiceProvider(false);

            var runner = provider.GetRequiredService<IMigrationRunner>();
            runner.MigrateUp();
        }
        #endregion
    }
}
