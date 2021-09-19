using CastIt.Application.Common.Utils;
using CastIt.Domain.Entities;
using CastIt.Infrastructure.Models;
using CastIt.Server.Interfaces;
using CastIt.Server.Migrations;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CastIt.Server.Services
{
    public class AppDataService : IAppDataService
    {
        private readonly string _connectionString;
        private readonly IFreeSql _db;

        public AppDataService()
        {
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

        public async Task<FileItem> AddFile(
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
            return file;
        }

        public async Task<List<FileItem>> AddFiles(List<FileItem> files)
        {
            var list = new List<FileItem>();
            foreach (var file in files)
            {
                file.Id = await _db.Insert(file).ExecuteIdentityAsync();
                list.Add(file);
            }

            return list;
        }

        public async Task<PlayList> AddNewPlayList(string name, int position)
        {
            var playlist = new PlayList
            {
                CreatedAt = DateTime.Now,
                Name = name,
                Position = position,
            };
            playlist.Id = await _db.Insert(playlist).ExecuteIdentityAsync();
            return playlist;
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

        public Task<List<PlayList>> GetAllPlayLists()
        {
            return _db.Select<PlayList>().ToListAsync();
        }

        public Task<PlayList> GetPlayList(long id)
        {
            return _db.Select<PlayList>().Where(pl => pl.Id == id).FirstAsync();
        }

        public Task<List<FileItem>> GetAllFiles(long playlistId)
        {
            return _db.Select<FileItem>().Where(pl => pl.PlayListId == playlistId).ToListAsync();
        }

        public Task<FileItem> GetFile(long id)
        {
            return _db.Select<FileItem>().Where(f => f.Id == id).FirstAsync();
        }

        public Task<long> GetNumberOfFiles(long playlistId)
        {
            return _db.Select<FileItem>().Where(f => f.PlayListId == playlistId).CountAsync();
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

        public async Task SavePlayListChanges(List<ServerPlayList> playLists)
        {
            if (playLists.Count == 0)
                return;

            foreach (var playList in playLists)
            {
                await _db.Update<PlayList>(playList.Id)
                    .Set(p => p.Position, playList.Position)
                    .Set(p => p.Shuffle, playList.Shuffle)
                    .Set(p => p.Loop, playList.Loop)
                    .ExecuteAffrowsAsync();
            }
        }

        public async Task SaveFileChanges(List<ServerFileItem> files)
        {
            if (files.Count == 0)
                return;

            foreach (var file in files)
            {
                await _db.Update<FileItem>(file.Id)
                    .Set(f => f.PlayedPercentage, file.PlayedPercentage)
                    .Set(f => f.Position, file.Position)
                    .ExecuteAffrowsAsync();
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
