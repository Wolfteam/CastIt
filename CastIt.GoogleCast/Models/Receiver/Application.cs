using System.Collections.Generic;

namespace CastIt.GoogleCast.Models.Receiver
{
    public class Application
    {
        public string AppId { get; set; }
        public string DisplayName { get; set; }
        public bool IsIdleScreen { get; set; }
        public List<Namespace> Namespaces { get; set; }
            = new List<Namespace>();
        public string SessionId { get; set; }
        public string StatusText { get; set; }
        public string TransportId { get; set; }
    }
}
