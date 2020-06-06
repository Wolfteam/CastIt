using CastIt.GoogleCast.Models.Media;
using System;

namespace CastIt.GoogleCast.Models.Events
{
    public class MediaStatusChangedArgs : EventArgs
    {
        public MediaStatus Status { get; set; }

        public MediaStatusChangedArgs(MediaStatus status)
        {
            Status = status;
        }
    }
}
