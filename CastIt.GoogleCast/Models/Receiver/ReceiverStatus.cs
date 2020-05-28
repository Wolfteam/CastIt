using System.Collections.Generic;

namespace CastIt.GoogleCast.Models.Receiver
{
    public class ReceiverStatus
    {
        public List<Application> Applications { get; set; }
            = new List<Application>();

        public Volume Volume { get; set; }
    }
}
