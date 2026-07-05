using CastIt.Domain.Interfaces;
using System;

namespace CastIt.Domain.Entities
{
    public class CustomEndpoint : IBaseEntity
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
