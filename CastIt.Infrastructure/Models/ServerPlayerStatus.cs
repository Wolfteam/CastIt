using CastIt.GoogleCast;

namespace CastIt.Infrastructure.Models
{
    public class ServerPlayerStatus
    {
        public PlayerStatus PlayerStatus { get; set; }
        public ServerFileItem CurrentFileItem { get; set; }
    }
}
