using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WOL.Models;
using WOL.Services.Interface;
using WOL.View;
using WOL.ViewModels;

namespace WOL.Services
{
    public class RemoteExplorerService : IRemoteExplorerService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public RemoteExplorerService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task<Tuple<bool?, List<EntryDto>>> ShowRemoteExplorerDialogAsync(Device device)
        {
            var scope = _scopeFactory.CreateScope();
            try
            {
                var remoteExplorerViewModel = scope.ServiceProvider.GetRequiredService<RemoteExplorerViewModel>();
                var iniService = scope.ServiceProvider.GetRequiredService<IIniService>();

                var picker = new RemoteExplorerView
                {
                    Owner = System.Windows.Application.Current?.MainWindow,
                    DataContext = remoteExplorerViewModel
                };

                remoteExplorerViewModel.ClientHost = device.IP;
                remoteExplorerViewModel.ClientPort = iniService.FileSelectPort > 0 ? iniService.FileSelectPort : 6060;

                await remoteExplorerViewModel.ConnectAsync();
                bool? result = picker.ShowDialog();
                List<EntryDto> selectedFiles = result == true ? remoteExplorerViewModel.SelectedFiles : [];

                return Tuple.Create(result, selectedFiles);
            }
            finally
            {
                if (scope is IAsyncDisposable asyncDisposable)
                {
                    await asyncDisposable.DisposeAsync();
                }
            }
        }
    }
}
