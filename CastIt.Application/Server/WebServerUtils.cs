﻿using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace CastIt.Application.Server
{
    public static class WebServerUtils
    {
        public const string ServerProcessName = "CastIt.Server";
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

            return process.Start();
        }
    }
}
