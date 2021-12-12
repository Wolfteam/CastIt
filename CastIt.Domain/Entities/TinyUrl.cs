using CastIt.Domain.Interfaces;
using System;

namespace CastIt.Domain.Entities
{
    public class TinyUrl : IBaseEntity
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string Base64 { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
