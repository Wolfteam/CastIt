using CastIt.Interfaces;
using FreeSql.DataAnnotations;
using System;

namespace CastIt.Models.Entities
{
    public class FileItem : IBaseEntity
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public long Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public double TotalSeconds { get; set; }

        public string Path { get; set; }

        public int Position { get; set; }

        public double PlayedPercentage { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public long PlayListId { get; set; }

        public string Hash { get; set; }
    }
}
