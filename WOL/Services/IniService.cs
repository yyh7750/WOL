using System;
using System.IO;
using System.Reflection;
using WOL.Services.Interface;

namespace WOL.Services
{
    public class IniService : IIniService
    {
        public int HeartbeatRecvPort { get; private set; }
        public int ShutdownSendPort { get; private set; }
        public int ProgramSignalPort { get; private set; }
        public int FileSelectPort { get; private set; }
        public int CommandListenPort { get; private set; }

        private readonly string _configPath;

        public IniService()
        {
            // Default values
            HeartbeatRecvPort = 12359;
            ShutdownSendPort = 12357;
            ProgramSignalPort = 30020;
            FileSelectPort = 6060;
            CommandListenPort = 30030;

            string exePath = Assembly.GetExecutingAssembly().Location;
            string exeDir = Path.GetDirectoryName(exePath) ?? ".";
            _configPath = Path.Combine(exeDir, "config.ini");

            LoadConfig();
        }

        public void LoadConfig()
        {
            if (!File.Exists(_configPath))
            {
                CreateDefaultConfig();
                return;
            }

            string[] lines = File.ReadAllLines(_configPath);
            foreach (string line in lines)
            {
                string[] parts = line.Split('=');
                if (parts.Length == 2)
                {
                    string key = parts[0].Trim();
                    string value = parts[1].Trim();

                    if (key.Equals("HEARTBEAT_RECV_PORT", StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(value, out int port)) HeartbeatRecvPort = port;
                    }
                    else if (key.Equals("SHUTDOWN_SEND_PORT", StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(value, out int port)) ShutdownSendPort = port;
                    }
                    else if (key.Equals("PROGRAM_SIGNAL_PORT", StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(value, out int port)) ProgramSignalPort = port;
                    }
                    else if (key.Equals("FILE_SELECT_PORT", StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(value, out int port)) FileSelectPort = port;
                    }
                    else if (key.Equals("COMMAND_LISTEN_PORT", StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(value, out int port)) CommandListenPort = port;
                    }
                }
            }
        }

        public void CreateDefaultConfig()
        {
            using (StreamWriter writer = new(_configPath))
            {
                writer.WriteLine("[CONFIG]");
                writer.WriteLine($"HEARTBEAT_RECV_PORT = {HeartbeatRecvPort}");
                writer.WriteLine($"SHUTDOWN_SEND_PORT = {ShutdownSendPort}");
                writer.WriteLine($"PROGRAM_SIGNAL_PORT = {ProgramSignalPort}");
                writer.WriteLine($"FILE_SELECT_PORT = {FileSelectPort}");
                writer.WriteLine($"COMMAND_LISTEN_PORT = {CommandListenPort}");
            }
        }
    }
}
