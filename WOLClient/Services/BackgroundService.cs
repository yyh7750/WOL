using System.Net.Sockets;
using System.Timers;
using System.Diagnostics;

namespace WOLClient.Services
{
    public class BackgroundService
    {
        private const uint SHUTDOWN_MESSAGE = 0x010;
        private const uint ALIVE_MESSAGE = 0x020;
        private const int FILE_SERVER_PORT_DEFAULT = 6060;
        private const int HEARTBEAT_INTERVAL_MS = 1000;

        private UdpClient? _udpListener;
        private System.Timers.Timer? _timer;
        private TcpFileServer? _fileServer;
        private readonly CancellationTokenSource _cts;
        private readonly IniService _iniService;

        public BackgroundService()
        {
            _cts = new CancellationTokenSource();
            _iniService = new IniService();
        }

        public void Start()
        {
            Task.Run(() => ListenForShutdownMessages(_cts.Token));

            _timer = new System.Timers.Timer(HEARTBEAT_INTERVAL_MS);
            _timer.Elapsed += SendHeartbeat;
            _timer.AutoReset = true;
            _timer.Enabled = true;

            // start TCP file server for remote file picking
            _fileServer = new TcpFileServer();
            int port = _iniService.FileSelectPort > 0 ? _iniService.FileSelectPort : FILE_SERVER_PORT_DEFAULT;
            _fileServer.Start(port);
        }

        public void Stop()
        {
            _cts.Cancel();
            _timer?.Stop();
            _timer?.Dispose();
            _udpListener?.Close();
            _udpListener?.Dispose();
            _fileServer?.Stop();
        }

        private async Task ListenForShutdownMessages(CancellationToken cancellationToken)
        {
            _udpListener = new UdpClient(_iniService.ShutdownListenPort);

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    UdpReceiveResult result = await _udpListener.ReceiveAsync(cancellationToken);
                    byte[] receivedBytes = result.Buffer;

                    if (receivedBytes.Length == 4)
                    {
                        uint message = BitConverter.ToUInt32(receivedBytes, 0);
                        if (message == SHUTDOWN_MESSAGE)
                        {
                            HandleShutdownRequest();
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // This is expected when the task is cancelled.
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in ListenForShutdownMessages: {ex.Message}");
            }
            finally
            {
                _udpListener?.Close();
            }
        }

        private void SendHeartbeat(object? sender, ElapsedEventArgs e)
        {
            try
            {
                using UdpClient udpClient = new();
                byte[] heartbeatBytes = BitConverter.GetBytes(ALIVE_MESSAGE);
                udpClient.Send(heartbeatBytes, heartbeatBytes.Length, _iniService.ServerIp, _iniService.SendPort);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in SendHeartbeat: {ex.Message}");
            }
        }

        private void HandleShutdownRequest()
        {
            try
            {
                Process.Start("shutdown", "/s /t 0"); // /s for shutdown, /t 0 for immediate
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in HandleShutdownRequest: {ex.Message}");
            }
        }
    }
}
