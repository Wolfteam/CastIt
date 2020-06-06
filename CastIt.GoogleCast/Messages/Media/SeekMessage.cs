using CastIt.GoogleCast.Messages.Base;

namespace CastIt.GoogleCast.Messages.Media
{
    internal class SeekMessage : MediaSessionMessage
    {
        public double CurrentTime { get; set; }
    }
}
