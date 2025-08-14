using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WOL.Models;

namespace WOL.Services.Interface
{
    public interface IDeviceService
    {
        void SetCurrentProject(Project project);
        void WakeAllDevices(Project project);
        void ShutdownAllDevices(Project project);

        event Action<Device>? DeviceStatusChanged;
    }
}
