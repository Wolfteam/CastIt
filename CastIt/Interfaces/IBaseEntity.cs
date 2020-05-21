using System;

namespace CastIt.Interfaces
{
    public interface IBaseEntity
    {
        long Id { get; set; }
        DateTime CreatedAt { get; set; }
        DateTime? UpdatedAt { get; set; }
    }
}
