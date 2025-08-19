using System;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WOL.Helpers.Interface;

namespace WOL.Helpers
{
	public sealed class TcpJsonClient : ITcpJsonClient
	{
		private readonly TcpClient _client = new();
		private NetworkStream? _stream;

		public async Task ConnectAsync(string host, int port, CancellationToken ct = default)
		{
			await _client.ConnectAsync(host, port, ct);
			_stream = _client.GetStream();
		}

		public async Task<T?> SendAsync<T>(object request, CancellationToken ct = default)
		{
			if (_stream is null) throw new InvalidOperationException("Not connected");
			var payload = JsonSerializer.SerializeToUtf8Bytes(request);
			var len = new byte[] { (byte)(payload.Length >> 24), (byte)(payload.Length >> 16), (byte)(payload.Length >> 8), (byte)payload.Length };
			await _stream.WriteAsync(len, ct);
			await _stream.WriteAsync(payload, ct);
			await _stream.FlushAsync(ct);

			var lenBuf = new byte[4];
			await ReadExactAsync(lenBuf, 4, ct);
			int rlen = (lenBuf[0] << 24) | (lenBuf[1] << 16) | (lenBuf[2] << 8) | lenBuf[3];

			var data = new byte[rlen];
			await ReadExactAsync(data, rlen, ct);
			return JsonSerializer.Deserialize<T>(data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		}

		private async Task ReadExactAsync(byte[] buf, int len, CancellationToken ct)
		{
			if (_stream is null) throw new InvalidOperationException("Not connected");
			int off = 0;
			while (off < len)
			{
				int r = await _stream.ReadAsync(buf.AsMemory(off, len - off), ct);
				if (r <= 0) throw new EndOfStreamException();
				off += r;
			}
		}

		public async ValueTask DisposeAsync()
		{
			if (_stream != null) await _stream.DisposeAsync();
			_client.Dispose();
		}
	}
}


