using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;

namespace WOLClient.Services
{
    public class TcpFileServer
    {
        private TcpListener? _listener;
        private bool _running;

        public void Start(int port)
        {
            if (_running) return;
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            _running = true;
            _ = AcceptLoop();
        }

        public void Stop()
        {
            _running = false;
            _listener?.Stop();
        }

        private async Task AcceptLoop()
        {
            if (_listener == null) return;
            while (_running)
            {
                try
                {
                    var client = await _listener.AcceptTcpClientAsync();
                    _ = Task.Run(() => HandleClient(client));
                }
                catch { if (!_running) break; }
            }
        }

        private static async Task HandleClient(TcpClient client)
        {
            using TcpClient _ = client;
            using var stream = client.GetStream();
            try
            {
                while (true)
                {
                    var lenBuf = new byte[4];
                    if (!await ReadExactAsync(stream, lenBuf, 4)) break;
                    int len = (lenBuf[0] << 24) | (lenBuf[1] << 16) | (lenBuf[2] << 8) | lenBuf[3];
                    var data = new byte[len];
                    if (!await ReadExactAsync(stream, data, len)) break;

                    var doc = await JsonDocument.ParseAsync(new MemoryStream(data));
                    var type = doc.RootElement.GetProperty("Type").GetString();

                    object? response = type switch
                    {
                        "Roots" => GetRoots().ToArray(),
                        "List" => List(
                            doc.RootElement.GetProperty("Path").GetString() ?? "C:\\",
                            doc.RootElement.TryGetProperty("Skip", out var s) ? s.GetInt32() : 0,
                            doc.RootElement.TryGetProperty("Take", out var t) ? t.GetInt32() : 500
                        ).ToArray(),
                        "Stat" => Stat(doc.RootElement.GetProperty("Path").GetString() ?? string.Empty),
                        _ => new { error = "BadRequest", message = "Unknown Type" }
                    };

                    var payload = JsonSerializer.SerializeToUtf8Bytes(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                    var outLen = new byte[] { (byte)(payload.Length >> 24), (byte)(payload.Length >> 16), (byte)(payload.Length >> 8), (byte)payload.Length };
                    await stream.WriteAsync(outLen);
                    await stream.WriteAsync(payload);
                    await stream.FlushAsync();
                }
            }
            catch { }
        }

        private static async Task<bool> ReadExactAsync(NetworkStream s, byte[] buf, int len)
        {
            int off = 0;
            while (off < len)
            {
                int r = await s.ReadAsync(buf.AsMemory(off, len - off));
                if (r <= 0) return false;
                off += r;
            }
            return true;
        }

        private static System.Collections.Generic.IEnumerable<object> GetRoots()
        {
            foreach (var d in DriveInfo.GetDrives().Where(d => d.IsReady))
                yield return new { fullPath = d.Name, name = d.Name, isDirectory = true, size = (long?)null, modifiedUtc = DateTime.UtcNow };
            yield return new { fullPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop), name = "Desktop", isDirectory = true, size = (long?)null, modifiedUtc = DateTime.UtcNow };
            yield return new { fullPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),"Downloads"), name = "Downloads", isDirectory = true, size = (long?)null, modifiedUtc = DateTime.UtcNow };
            yield return new { fullPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), name = "Program Files", isDirectory = true, size = (long?)null, modifiedUtc = DateTime.UtcNow };
        }

        private static System.Collections.Generic.IEnumerable<object> List(string path, int skip, int take)
        {
            System.Collections.Generic.IEnumerable<object> dirs, files;
            try { dirs = Directory.EnumerateDirectories(path).Select(p => Stat(p, true)).Where(x => x != null)!; }
            catch { dirs = System.Linq.Enumerable.Empty<object>(); }
            try { files = Directory.EnumerateFiles(path).Select(p => Stat(p, false)).Where(x => x != null)!; }
            catch { files = System.Linq.Enumerable.Empty<object>(); }

            return dirs.Concat(files)
                .OrderBy(x => ((dynamic)x).isDirectory ? 0 : 1)
                .ThenBy(x => ((dynamic)x).name)
                .Skip(skip).Take(take);
        }

        private static object? Stat(string path, bool? isDirHint = null)
        {
            try
            {
                bool isDir = isDirHint ?? Directory.Exists(path);
                if (isDir)
                {
                    var di = new DirectoryInfo(path);
                    return new { fullPath = di.FullName, name = di.Name, isDirectory = true, size = (long?)null, modifiedUtc = di.LastWriteTimeUtc };
                }
                else
                {
                    var fi = new FileInfo(path);
                    return new { fullPath = fi.FullName, name = fi.Name, isDirectory = false, size = (long?)fi.Length, modifiedUtc = fi.LastWriteTimeUtc };
                }
            }
            catch { return null; }
        }
    }
}


