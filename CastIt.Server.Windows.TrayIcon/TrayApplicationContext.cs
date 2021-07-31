using System.Windows.Forms;

namespace CastIt.Server.Windows.TrayIcon
{
    public class TrayApplicationContext : ApplicationContext
    {
        private readonly TrayIcon _trayIcon;
        public TrayApplicationContext()
        {
            _trayIcon = new TrayIcon();
        }
    }
}
