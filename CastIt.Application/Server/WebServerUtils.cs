using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Principal;

namespace CastIt.Application.Server
{
    public static class WebServerUtils
    {
        public const string ServerProcessName = "CastIt.Server";
        public const string ServerFolderName = "Server";
        public static string FullServerProcessName = $"{ServerProcessName}.exe";

        public static string GetWebServerIpAddress()
        {
            if (!IsServerAlive())
                return null;
            var port = GetServerPort();
            if (port.HasValue)
                return GetWebServerIpAddress(port.Value);
            return null;
        }

        public static string GetWebServerIpAddress(int port)
        {
            string localIp = GetLocalIpAddress();
            return $"http://{localIp}:{port}";
        }

        public static string GetLocalIpAddress()
        {
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
            socket.Connect("8.8.8.8", 65530);
            var endPoint = socket.LocalEndPoint as IPEndPoint;
            var localIp = endPoint?.Address.ToString();
            return localIp;
        }

        public static int GetOpenPort(int startPort = AppWebServerConstants.DefaultPort)
        {
            var properties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpEndPoints = properties.GetActiveTcpListeners();
            var usedPorts = tcpEndPoints.Select(p => p.Port).ToList();
            return Enumerable.Range(startPort, 99).FirstOrDefault(port => !usedPorts.Contains(port));
        }

        public static Process GetServerProcess()
        {
            var existingProcess = Process.GetProcessesByName(ServerProcessName);
            return existingProcess.FirstOrDefault();
        }

        public static bool IsServerAlive()
        {
            var existingProcess = GetServerProcess();
            return existingProcess != null;
        }

        public static void KillServerProcess()
        {
            var existingProcess = GetServerProcess();
            existingProcess?.Kill(true);
        }

        public static int? GetServerPort()
        {
            var process = ProcessPorts.ProcessPortMap.Find(f => f.ProcessName == ServerProcessName);
            return process?.PortNumber;
        }

        public static bool StartServer(string escapedArgs, string exePath)
        {
            var process = new Process
            {
                EnableRaisingEvents = true,
                StartInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = exePath,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Arguments = escapedArgs,
                },
            };

            try
            {
                return process.Start();
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool StartServer()
        {
            string path = GetServerPhysicalPath();
            return StartServer(path);
        }

        public static bool StartServer(string exePath)
        {
            string args = $"{AppWebServerConstants.PortArgument} {GetOpenPort()}";
            return StartServer(args, exePath);
        }

        public static string GetServerPhysicalPath()
        {
            string dir = Directory.GetCurrentDirectory();
            return GetServerPhysicalPath(dir);
        }

        public static string GetServerPhysicalPath(string from)
        {
            //C:\Program Files\CastIt
            string parentDir = Directory.GetParent(from)!.ToString();
            string path = Path.Combine(parentDir, ServerFolderName, FullServerProcessName);
            //#if DEBUG
            //            path = "D:\\Proyectos\\CastIt\\CastIt.Server\\bin\\Debug\\net5.0\\CastIt.Server.exe";
            //#endif
            return path;
        }

        public static bool IsElevated()
        {
            if (OperatingSystem.IsWindows())
            {
                var id = WindowsIdentity.GetCurrent();
                return id.Owner != id.User;
            }

            return false;
        }
    }
}
