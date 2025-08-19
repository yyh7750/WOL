using System.IO;
using System.Reflection;

namespace WOLClient.Services
{
    public class IniService
    {
        public string ServerIp { get; private set; }
        public int SendPort { get; private set; }
        public int ShutdownListenPort { get; private set; }
        public int FileSelectPort { get; private set; }

        private readonly string _configPath;

        public IniService()
        {
            ServerIp = string.Empty;
            SendPort = 0;
            FileSelectPort = 6060;

            string exePath = Assembly.GetExecutingAssembly().Location;
            string exeDir = Path.GetDirectoryName(exePath) ?? ".";
            _configPath = Path.Combine(exeDir, "config.ini");

            LoadConfig();
        }

        private void LoadConfig()
        {
            // 기본값
            ServerIp = "192.168.1.2";
            SendPort = 12359;
            ShutdownListenPort = 12357;
            FileSelectPort = 6060;

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

                    if (key.Equals("SERVER_IP", StringComparison.OrdinalIgnoreCase))
                    {
                        ServerIp = value;
                    }
                    else if (key.Equals("SEND_PORT", StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(value, out int port))
                        {
                            SendPort = port;
                        }
                    }
                    else if (key.Equals("SHUTDOWN_LISTEN_PORT", StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(value, out int port))
                        {
                            ShutdownListenPort = port;
                        }
                    }
                    else if (key.Equals("FILE_SELECT_PORT", StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(value, out int port))
                        {
                            FileSelectPort = port;
                        }
                    }
                }
            }
        }

        private void CreateDefaultConfig()
        {
            using (StreamWriter writer = new(_configPath))
            {
                writer.WriteLine("[CONFIG]");
                writer.WriteLine("SERVER_IP = 192.168.1.2");
                writer.WriteLine("SEND_PORT = 12359");
                writer.WriteLine("SHUTDOWN_LISTEN_PORT = 12357");
                writer.WriteLine("FILE_SELECT_PORT = 6060");
            }
        }
    }
}
