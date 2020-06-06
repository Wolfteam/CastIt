using CastIt.GoogleCast.Interfaces.Messages;

namespace CastIt.GoogleCast.Models
{
    public class AppMessage
    {
        public string NameSpace { get; set; }
        public IMessage Message { get; set; }
        public string DestinationId { get; set; }
    }
}
