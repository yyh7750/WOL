using System;
using System.IO;
using System.Reflection;

namespace WOL.Services
{
	public class IniService : Interface.IIniService
	{
		public string ServerIp { get; private set; } = string.Empty;
		public int HeartbeatRecvPort { get; private set; }
		public int ShutdownSendPort { get; private set; }
		public int FileSelectPort { get; private set; } = 6060;

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
			// defaults
			ServerIp = "192.168.1.2";
			HeartbeatRecvPort = 12358;
			ShutdownSendPort = 12357;
			FileSelectPort = 6060;

			if (!File.Exists(_configPath))
			{
				CreateDefaultConfig();
				return;
			}

			foreach (var line in File.ReadAllLines(_configPath))
			{
				var parts = line.Split('=');
				if (parts.Length != 2) continue;
				var key = parts[0].Trim();
				var value = parts[1].Trim();
				if (key.Equals("SERVER_IP", StringComparison.OrdinalIgnoreCase)) ServerIp = value;
				else if (key.Equals("HEARTBEAT_RECV_PORT", StringComparison.OrdinalIgnoreCase) && int.TryParse(value, out var p1)) HeartbeatRecvPort = p1;
				else if (key.Equals("SHUTDOWN_SEND_PORT", StringComparison.OrdinalIgnoreCase) && int.TryParse(value, out var p2)) ShutdownSendPort = p2;
				else if (key.Equals("FILE_SELECT_PORT", StringComparison.OrdinalIgnoreCase) && int.TryParse(value, out var p3)) FileSelectPort = p3;
			}
		}

		public void CreateDefaultConfig()
		{
			using var writer = new StreamWriter(_configPath);
			writer.WriteLine("[CONFIG]");
			writer.WriteLine("SERVER_IP = 192.168.1.2");
			writer.WriteLine("HEARTBEAT_RECV_PORT = 12358");
			writer.WriteLine("SHUTDOWN_SEND_PORT = 12357");
			writer.WriteLine("FILE_SELECT_PORT = 6060");
		}
	}
}
