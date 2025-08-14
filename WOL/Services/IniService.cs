using System;
using System.IO;
using System.Reflection;
using WOL.Services.Interface;

namespace WOL.Services
{
    public class IniService : IIniService
    {
        public int ShutdownSendPort { get; private set; }
        public int HeartbeatRecvPort { get; private set; }

        private readonly string _configPath;

        public IniService()
        {
            string exePath = Assembly.GetExecutingAssembly().Location;
            string exeDir = Path.GetDirectoryName(exePath) ?? ".";
            _configPath = Path.Combine(exeDir, "config.ini");

            LoadConfig();
        }

        public void LoadConfig()
        {
            // 기본값
            ShutdownSendPort = 12357;
            HeartbeatRecvPort = 12359;

            if (!File.Exists(_configPath))
            {
                CreateDefaultConfig();
                return;
            }

            var lines = File.ReadAllLines(_configPath);
            foreach (var line in lines)
            {
                var parts = line.Split('=');
                if (parts.Length == 2)
                {
                    string key = parts[0].Trim();
                    string value = parts[1].Trim();

                    if (key.Equals("SHUTDOWN_SEND_PORT", StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(value, out int port))
                        {
                            ShutdownSendPort = port;
                        }
                    }
                    else if (key.Equals("HEARTBEAT_RECV_PORT", StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(value, out int port))
                        {
                            HeartbeatRecvPort = port;
                        }
                    }
                }
            }
        }

        public void CreateDefaultConfig()
        {
            using (var writer = new StreamWriter(_configPath))
            {
                writer.WriteLine("[CONFIG]");
                writer.WriteLine("SHUTDOWN_SEND_PORT = 12357");
                writer.WriteLine("HEARTBEAT_RECV_PORT = 12359");
            }
        }
    }
}
