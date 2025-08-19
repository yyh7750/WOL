using System;
using System.Threading;
using System.Threading.Tasks;

namespace WOL.Helpers.Interface
{
    public interface ITcpJsonClient : IAsyncDisposable
    {
        Task ConnectAsync(string host, int port, CancellationToken ct = default);
        Task<T?> SendAsync<T>(object request, CancellationToken ct = default);
    }
}
