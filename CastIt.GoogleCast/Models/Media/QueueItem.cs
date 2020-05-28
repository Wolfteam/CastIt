using System.Collections.Generic;

namespace CastIt.GoogleCast.Models.Media
{
    public class QueueItem
    {
        public List<int> ActiveTrackIds { get; set; } 
            = new List<int>();

        public bool Autoplay { get; set; } = true;

        public int? ItemId { get; set; }

        public MediaInformation Media { get; set; }

        public int? StartTime { get; set; }
    }
}
