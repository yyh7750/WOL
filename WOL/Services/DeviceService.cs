using WOL.Models;
using WOL.Services.Interface;

namespace WOL.Services
{
    public class DeviceService : IDeviceService
    {
        private readonly IWakeOnLanService _wakeOnLanService;

        public DeviceService(IWakeOnLanService wakeOnLanService)
        {
            _wakeOnLanService = wakeOnLanService;
        }

        public void WakeAllDevices(Project project)
        {
            if (project == null || project.Devices == null) return;

            foreach (Device device in project.Devices)
            {
                _wakeOnLanService.WakeUpAsync(device.MAC);
            }

            // TODO make WOL Client
            // client에서 WakeUp을 완료하여 ping을 쏘면 Device의 상태를 업데이트할 수 있도록 구현 필요
        }

        public void ShutdownAllDevices(Project project)
        {
            if (project == null || project.Devices == null) return;

            foreach (Device device in project.Devices)
            {
                // TODO make WOL Client
                // send udp shutdown command to device
            }
        }
    }
}
