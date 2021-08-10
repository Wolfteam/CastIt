using CastIt.Application.Common.Utils;
using CastIt.Application.Server;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Windows.Forms;

namespace CastIt.Server.Windows.TrayIcon
{
    public class TrayIcon
    {
        private readonly ServiceController _serviceController;

        private ToolStripMenuItem _menuItemStart;
        private ToolStripMenuItem _menuItemStop;
        private ToolStripMenuItem _menuItemOpenUrl;
        private ToolStripMenuItem _menuItemLogFolder;
        private ToolStripMenuItem _menuItemExit;
        private NotifyIcon _trayIcon;

        public TrayIcon()
        {
            _serviceController = Array.Find(ServiceController.GetServices(), s => s.ServiceName == WebServerUtils.ServerProcessName);

            CreateTrayIcon();
            CheckShowServiceNotElevatedWarning();
        }

        private bool CheckShowServiceNotElevatedWarning()
        {
            if (!WebServerUtils.IsElevated())
            {
                MessageBox.Show("To start / stop the server, this application must be ran as Administrator.");
                return true;
            }
            return false;
        }

        private void CreateTrayIcon()
        {
            _menuItemStart = new ToolStripMenuItem("Start Server", null, Start);
            _menuItemStop = new ToolStripMenuItem("Stop Server", null, Stop);
            _menuItemLogFolder = new ToolStripMenuItem("Show App Folder", null, ShowAppFolder);
            _menuItemOpenUrl = new ToolStripMenuItem("Open Url", null, OpenUrl);
            _menuItemExit = new ToolStripMenuItem("Exit", null, Exit);

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add(_menuItemStart);
            contextMenu.Items.Add(_menuItemStop);
            contextMenu.Items.Add(_menuItemLogFolder);
            contextMenu.Items.Add(_menuItemOpenUrl);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(_menuItemExit);
            contextMenu.Opening += ContextMenuOnPopup;

            var entryAssembly = Assembly.GetEntryAssembly();
            var iconPath = entryAssembly?.Location;
            if (string.IsNullOrWhiteSpace(iconPath))
            {
                //Two things, first the CurrentDirectory seems to be Server...
                //second, the GetEntryAssembly is returning null on release...
                iconPath = Path.Combine(Directory.GetCurrentDirectory(), "TrayIcon", "CastIt.Server.Windows.TrayIcon.exe");
            }
            var icon = System.Drawing.Icon.ExtractAssociatedIcon(iconPath);
            _trayIcon = new NotifyIcon
            {
                Icon = icon,
                ContextMenuStrip = contextMenu,
                Visible = true,
                Text = WebServerUtils.ServerProcessName
            };
            _trayIcon.ShowBalloonTip(1000, null, WebServerUtils.ServerProcessName, ToolTipIcon.None);
        }

        private void ContextMenuOnPopup(object sender, EventArgs e)
        {
            _serviceController.Refresh();
            bool isElevated = WebServerUtils.IsElevated();
            _menuItemStart.Enabled = isElevated && _serviceController.Status == ServiceControllerStatus.Stopped;
            _menuItemStop.Enabled = isElevated && _serviceController.Status == ServiceControllerStatus.Running;
        }

        private void Start(object sender, EventArgs e)
        {
            if (WebServerUtils.IsServerAlive())
            {
                return;
            }
            _serviceController.Start();
        }

        private void Stop(object sender, EventArgs e)
        {
            _serviceController.Stop();
        }

        public void OpenUrl(object sender, EventArgs e)
        {
            string ip = WebServerUtils.GetWebServerIpAddress();
            if (string.IsNullOrWhiteSpace(ip))
            {
                MessageBox.Show("Server is not running", "Error");
                return;
            }
            Process.Start("explorer.exe", $"{ip}/swagger/index.html");
        }

        public void ShowAppFolder(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", AppFileUtils.GetBaseAppFolder());
        }

        private void Exit(object sender, EventArgs e)
        {
            _trayIcon.Visible = false;
            System.Windows.Forms.Application.Exit();
        }
    }
}
