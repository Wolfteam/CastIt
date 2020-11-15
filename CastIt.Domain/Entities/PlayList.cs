using System;
using CastIt.Domain.Interfaces;

namespace CastIt.Domain.Entities
{
    public class PlayList : IBaseEntity
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public int Position { get; set; }

        public bool Loop { get; set; }

        public bool Shuffle { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
