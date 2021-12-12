using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace CastIt.Shared.Server
{
    /// <summary>
    /// https://stackoverflow.com/questions/1675077/how-do-i-get-process-name-of-an-open-port-in-c
    /// </summary>
    internal static class ProcessPorts
    {
        public static List<ProcessPort> ProcessPortMap => GetNetStatPorts();

        private static List<ProcessPort> GetNetStatPorts()
        {
            var processPorts = new List<ProcessPort>();

            try
            {
                using var proc = new Process();
                var startInfo = new ProcessStartInfo
                {
                    FileName = "netstat.exe",
                    Arguments = "-a -n -o",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                proc.StartInfo = startInfo;
                proc.Start();

                var standardOutput = proc.StandardOutput;
                var standardError = proc.StandardError;

                string netStatContent = standardOutput.ReadToEnd() + standardError.ReadToEnd();
                string netStatExitStatus = proc.ExitCode.ToString();

                if (netStatExitStatus != "0")
                {
                    Console.WriteLine("NetStat command failed.   This may require elevated permissions.");
                }

                string[] netStatRows = Regex.Split(netStatContent, "\r\n");

                foreach (string netStatRow in netStatRows)
                {
                    string[] tokens = Regex.Split(netStatRow, "\\s+");
                    if (tokens.Length > 4 && (tokens[1].Equals("UDP") || tokens[1].Equals("TCP")))
                    {
                        string ipAddress = Regex.Replace(tokens[2], @"\[(.*?)\]", "1.1.1.1");
                        try
                        {
                            var processName = tokens[1] == "UDP"
                                ? GetProcessName(Convert.ToInt32(tokens[4]))
                                : GetProcessName(Convert.ToInt32(tokens[5]));

                            var processId = tokens[1] == "UDP"
                                ? Convert.ToInt32(tokens[4])
                                : Convert.ToInt32(tokens[5]);

                            var protocol = ipAddress.Contains("1.1.1.1") ? $"{tokens[1]}v6" : $"{tokens[1]}v4";
                            processPorts.Add(new ProcessPort(processName, processId, protocol, Convert.ToInt32(ipAddress.Split(':')[1])));
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Could not convert the following NetStat row to a Process to Port mapping.");
                            Console.WriteLine(netStatRow);
                        }
                    }
                    else
                    {
                        if (netStatRow.Trim().StartsWith("Proto") || netStatRow.Trim().StartsWith("Active") || string.IsNullOrWhiteSpace(netStatRow))
                            continue;
                        Console.WriteLine("Unrecognized NetStat row to a Process to Port mapping.");
                        Console.WriteLine(netStatRow);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return processPorts;
        }

        private static string GetProcessName(int processId)
        {
            string procName = "UNKNOWN";

            try
            {
                procName = Process.GetProcessById(processId).ProcessName;
            }
            catch
            {
                // ignored
            }

            return procName;
        }
    }

    internal class ProcessPort
    {
        public string ProcessPortDescription => $"{ProcessName} ({Protocol} port {PortNumber} pid {ProcessId})";

        public string ProcessName { get; }

        public int ProcessId { get; }

        public string Protocol { get; }

        public int PortNumber { get; }

        internal ProcessPort(string processName, int processId, string protocol, int portNumber)
        {
            ProcessName = processName;
            ProcessId = processId;
            Protocol = protocol;
            PortNumber = portNumber;
        }
    }
}
