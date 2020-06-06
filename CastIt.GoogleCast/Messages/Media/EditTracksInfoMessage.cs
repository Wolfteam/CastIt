using CastIt.GoogleCast.Messages.Base;
using CastIt.GoogleCast.Models.Media;
using System.Collections.Generic;

namespace CastIt.GoogleCast.Messages.Media
{
    internal class EditTracksInfoMessage : MediaSessionMessage
    {
        public List<int> ActiveTrackIds { get; set; }
            = new List<int>();

        public bool EnableTextTracks { get; set; }

        public string Language { get; set; }

        public TextTrackStyle TextTrackStyle { get; set; }
    }
}
