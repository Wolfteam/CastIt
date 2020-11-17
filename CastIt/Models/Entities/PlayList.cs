using CastIt.Interfaces;
using System;
using FreeSql.DataAnnotations;

namespace CastIt.Models.Entities
{
    public class PlayList : IBaseEntity
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public long Id { get; set; }

        public string Name { get; set; }

        public int Position { get; set; }

        public bool Loop { get; set; }

        public bool Shuffle { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
