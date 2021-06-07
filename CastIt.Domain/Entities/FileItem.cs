using CastIt.Domain.Interfaces;
using System;

namespace CastIt.Domain.Entities
{
    public class FileItem : IBaseEntity
    {
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

        public double PlayedSeconds
            => PlayedPercentage * TotalSeconds / 100;

        public bool CanStartPlayingFromCurrentPercentage
            => PlayedPercentage > 0 && PlayedPercentage < 100;

        public bool WasPlayed
            => PlayedPercentage > 0 && PlayedPercentage <= 100;
    }
}
