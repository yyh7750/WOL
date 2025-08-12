using WOL.Models;

namespace WOL.Services.Interface
{
    public interface IDeviceService
    {
        void WakeAllDevices(Project project);
        void ShutdownAllDevices(Project project);
    }
}
