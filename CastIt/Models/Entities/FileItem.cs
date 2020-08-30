using CastIt.Interfaces;
using SQLite;
using System;
using CastIt.Common;

namespace CastIt.Models.Entities
{
    public class FileItem : IBaseEntity
    {
        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }

        [MaxLength(AppConstants.MaxCharsPerString)]
        public string Name { get; set; }

        [NotNull, MaxLength(AppConstants.MaxCharsPerString)]
        public string Path { get; set; }

        [NotNull]
        public int Position { get; set; }

        [NotNull]
        public double PlayedPercentage { get; set; }

        [NotNull]
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [NotNull]
        public long PlayListId { get; set; }
    }
}
