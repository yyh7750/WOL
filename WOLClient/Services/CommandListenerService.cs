using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using WOLClient.Models.Dto;

namespace WOLClient.Services
{
    public class CommandListenerService
    {
        private readonly IniService _iniService;
        private TcpListener? _listener;
        private CancellationTokenSource? _cts;

        #region WinAPI
        private const int PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(int access, bool inherit, int processId);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool QueryFullProcessImageName(
            IntPtr hProcess, int flags, StringBuilder exeName, ref int size);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);
        #endregion

        public CommandListenerService(IniService iniService)
        {
            _iniService = iniService;
        }

        public void Start()
        {
            if (_cts != null && !_cts.IsCancellationRequested)
            {
                return; // Already running
            }

            _cts = new CancellationTokenSource();
            _listener = new TcpListener(IPAddress.Any, _iniService.CommandListenPort);
            Task.Run(() => ListenForCommands(_cts.Token), _cts.Token);
        }

        public void Stop()
        {
            _cts?.Cancel();
            _listener?.Stop();
            _cts?.Dispose();
        }

        private async Task ListenForCommands(CancellationToken token)
        {
            _listener?.Start();
            Debug.WriteLine($"Command listener started on port {_iniService.CommandListenPort}");

            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (_listener == null) break;
                    TcpClient client = await _listener.AcceptTcpClientAsync(token);
                    _ = HandleClientAsync(client, token);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when stopping
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in command listener: {ex.Message}");
            }
            finally
            {
                _listener?.Stop();
                Debug.WriteLine("Command listener stopped.");
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken token)
        {
            try
            {
                await using NetworkStream stream = client.GetStream();
                using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
                using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true);

                string? command = await reader.ReadLineAsync(token);
                Debug.WriteLine($"Received command: {command}");

                if (command == "GET_RUNNING_PROCESSES")
                {
                    var processes = GetRunningProcesses();
                    string jsonResponse = JsonSerializer.Serialize(processes);
                    await writer.WriteLineAsync(jsonResponse.AsMemory(), token);
                    await writer.FlushAsync(token);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error handling client: {ex.Message}");
            }
            finally
            {
                client.Close();
            }
        }

        private static string? TryGetExePath(Process p)
        {
            IntPtr h = IntPtr.Zero;
            try
            {
                h = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, false, p.Id);
                if (h == IntPtr.Zero) return null;

                var sb = new StringBuilder(1024);
                int size = sb.Capacity;
                if (QueryFullProcessImageName(h, 0, sb, ref size))
                    return sb.ToString(0, size);

                return null;
            }
            catch
            {
                return null;
            }
            finally
            {
                if (h != IntPtr.Zero) CloseHandle(h);
            }
        }

        public List<ProcessDto> GetRunningProcesses(
            bool currentSessionOnly = true,
            bool withMainWindowOnly = true,
            bool uniqueByPath = true,
            long minWorkingSetBytes = 20L * 1024 * 1024, // 20MB 이상
            string[]? namePrefixes = null,
            int? takeTop = null)
        {
            List<(ProcessDto dto, bool hasWindow, long ws, string? pathKey)> list = [];

            int currentSession = Process.GetCurrentProcess().SessionId;

            foreach (Process p in Process.GetProcesses())
            {
                try
                {
                    if (p.HasExited) continue;

                    if (currentSessionOnly && p.SessionId != currentSession)
                        continue;

                    bool hasWindow = p.MainWindowHandle != IntPtr.Zero;
                    if (withMainWindowOnly && !hasWindow)
                        continue;

                    if (minWorkingSetBytes > 0 && p.WorkingSet64 < minWorkingSetBytes)
                        continue;

                    if (namePrefixes != null && namePrefixes.Length > 0 &&
                        !namePrefixes.Any(pref => p.ProcessName.StartsWith(pref, StringComparison.OrdinalIgnoreCase)))
                        continue;

                    string? path = null;
                    try
                    {
                        // API 버전 (안되면 MainModule)
                        path = TryGetExePath(p) ?? p.MainModule?.FileName;
                    }
                    catch { /* ignore */ }

                    if (uniqueByPath)
                    {
                        string key = string.IsNullOrEmpty(path) ? $"__NO_PATH__::{p.ProcessName}" : path;
                        list.Add((new ProcessDto { Name = p.ProcessName, FilePath = path! }, hasWindow, p.WorkingSet64, key));
                    }
                    else
                    {
                        list.Add((new ProcessDto { Name = p.ProcessName, FilePath = path! }, hasWindow, p.WorkingSet64, null));
                    }
                }
                catch { /* skip */ }
                finally { p.Dispose(); }
            }

            IEnumerable<(ProcessDto dto, bool hasWindow, long ws, string? key)> query = list;

            if (uniqueByPath)
            {
                query = list
                    .GroupBy(x => x.pathKey, StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.OrderByDescending(x => x.ws).First());
            }

            query = query
                .OrderByDescending(x => x.hasWindow)
                .ThenByDescending(x => x.ws)
                .ThenBy(x => x.dto.Name, StringComparer.OrdinalIgnoreCase);

            if (takeTop is int n)
                query = query.Take(n);

            return query.Select(x => x.dto).ToList();
        }
    }
}
