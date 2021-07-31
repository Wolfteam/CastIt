using System;
using System.Threading;
using System.Windows.Forms;

namespace CastIt.Server.Windows.TrayIcon
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using var mutex = new Mutex(false, "Global\\CastIt.TrayIcon");
            if (!mutex.WaitOne(0, false))
            {
                MessageBox.Show("Instance already running");
                return;
            }

            //https://github.com/jellyfin/jellyfin-server-windows/blob/master/Jellyfin.Windows.Tray/TrayApplicationContext.cs
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
            System.Windows.Forms.Application.Run(new TrayApplicationContext());
        }
    }
}
