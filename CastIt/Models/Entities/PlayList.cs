using CastIt.Interfaces;
using System;
using System.Collections.Generic;

namespace CastIt.Models.Entities
{
    public class PlayList : IBaseEntity
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public ICollection<FileItem> Items { get;  set; } 
            = new List<FileItem>();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
