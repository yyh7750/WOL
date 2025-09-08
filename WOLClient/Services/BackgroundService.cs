using System.Net.Sockets;
using System.Timers;
using System.Diagnostics;
using System.Text.Json;
using System.Text;
using WOLClient.Models.Dto;
using System.IO;

namespace WOLClient.Services
{
    public class BackgroundService
    {
        private const uint SHUTDOWN_MESSAGE = 0x010;
        private const uint ALIVE_MESSAGE = 0x020;
        private const int FILE_SERVER_PORT_DEFAULT = 6060;
        private const int HEARTBEAT_INTERVAL_MS = 1000;

        private UdpClient? _shutdownUdpListener;
        private UdpClient? _programUdpListener;
        private System.Timers.Timer? _timer;

        private CancellationTokenSource? _cts;
        private Task? _shutdownTask;
        private Task? _programTask;

        private TcpFileServer? _fileServer;
        private readonly IniService _iniService;

        public BackgroundService()
        {
            _cts = new CancellationTokenSource();
            _iniService = new IniService();
        }

        public void Start()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            _shutdownTask = Task.Run(() => ListenForShutdownMessages(token), token);

            _timer = new System.Timers.Timer(HEARTBEAT_INTERVAL_MS);
            _timer.Elapsed += SendHeartbeat;
            _timer.AutoReset = true;
            _timer.Enabled = true;

            _fileServer = new TcpFileServer();
            int port = _iniService.FileSelectPort > 0 ? _iniService.FileSelectPort : FILE_SERVER_PORT_DEFAULT;
            _fileServer.Start(port);

            _programTask = Task.Run(() => ListenForProgramSignalMessages(token), token);
        }

        public async Task StopAsync()
        {
            _cts?.Cancel();

            if (_timer != null)
            {
                _timer.Enabled = false;
                _timer.Dispose();
                _timer = null;
            }

            _shutdownUdpListener?.Close();
            _programUdpListener?.Close();

            try
            {
                var tasks = new[] { _shutdownTask, _programTask }.Where(t => t != null)!;
                await Task.WhenAll(tasks!);
            }
            catch (OperationCanceledException ex)
            {
                Debug.WriteLine($"StopAsync Tasks cancelled: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"StopAsync Tasks error: {ex}");
            }
            finally
            {
                _shutdownTask = _programTask = null;
            }

            _shutdownUdpListener?.Dispose();
            _shutdownUdpListener = null;

            _programUdpListener?.Dispose();
            _programUdpListener = null;

            _fileServer?.Stop();
            _fileServer = null;

            _cts?.Dispose();
            _cts = null;
        }

        private async Task ListenForShutdownMessages(CancellationToken cancellationToken)
        {
            _shutdownUdpListener = new UdpClient(_iniService.ShutdownListenPort);

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    UdpReceiveResult result = await _shutdownUdpListener.ReceiveAsync(cancellationToken);
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
            catch (OperationCanceledException ex)
            {
                Debug.WriteLine($"ListenForShutdownMessages cancelled: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in ListenForShutdownMessages: {ex.Message}");
            }
        }

        private async Task ListenForProgramSignalMessages(CancellationToken cancellationToken)
        {
            _programUdpListener = new UdpClient(_iniService.ProgramSignalPort);

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    UdpReceiveResult result = await _programUdpListener.ReceiveAsync(cancellationToken);
                    byte[] receivedBytes = result.Buffer;

                    string json = Encoding.UTF8.GetString(receivedBytes);

                    try
                    {
                        ProgramDto? dto = JsonSerializer.Deserialize<ProgramDto>(json);
                        if (dto != null)
                        {
                            Debug.WriteLine($"[RECEIVED] IsStart={dto.IsStart}, Paths={string.Join(", ", dto.Path)}");

                            if (dto.IsStart)
                            {
                                try
                                {
                                    Process.Start(new ProcessStartInfo
                                    {
                                        FileName = dto.Path,
                                        UseShellExecute = true,
                                        Verb = "runas", //   
                                        WorkingDirectory = Path.GetDirectoryName(dto.Path)
                                    });
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine($"[ERROR] Failed to start program '{dto.Path}': {ex.Message}");
                                }
                            }
                            else
                            {
                                try
                                {
                                    string processName = Path.GetFileNameWithoutExtension(dto.Path);
                                    foreach (var proc in Process.GetProcessesByName(processName))
                                    {
                                        proc.Kill();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine($"[ERROR] Failed to stop program '{dto.Path}': {ex.Message}");
                                }

                            }
                        }
                        else
                        {
                            Debug.WriteLine("[ERROR] Deserialized ProgramDto is null.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[ERROR] {ex.Message}");
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                Debug.WriteLine($"ListenForProgramSignalMessages cancelled: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in ListenForProgramSignalMessages: {ex.Message}");
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