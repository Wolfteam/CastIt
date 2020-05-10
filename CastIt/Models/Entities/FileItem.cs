using CastIt.Interfaces;
using System;

namespace CastIt.Models.Entities
{
    public class FileItem : IBaseEntity
    {
        public long Id { get; set; }
        public int Position { get; set; }
        public string Path { get; set; }
        public double PlayedPercentage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public long PlayListId { get; set; }
        public PlayList PlayList { get; set; }
    }
}
