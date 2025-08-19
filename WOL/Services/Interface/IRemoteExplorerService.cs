using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WOL.Models;
using WOL.ViewModels;

namespace WOL.Services.Interface
{
    public interface IRemoteExplorerService
    {
        Task<Tuple<bool?, List<EntryDto>>> ShowRemoteExplorerDialogAsync(Device device);
    }
}
