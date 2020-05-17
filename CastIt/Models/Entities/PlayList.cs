using CastIt.Interfaces;
using SQLite;
using System;

namespace CastIt.Models.Entities
{
    public class PlayList : IBaseEntity
    {
        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }

        [NotNull]
        public string Name { get; set; }

        [NotNull]
        public int Position { get; set; }

        [NotNull]
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
