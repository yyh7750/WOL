using System;
using System.Collections.Generic;
using System.Timers;
using WOL.Models;
using WOL.Services.Interface;

namespace WOL.Services
{
    public class DeviceService : IDeviceService
    {
        private const int OFFLINE_TIMEOUT_MS = 5000;
        private readonly IWakeOnLanService _wakeOnLanService;
        private readonly IProgramService _programService;
        private readonly Dictionary<string, DateTime> _lastHeartbeatTimes;
        private Timer? _offlineCheckTimer;

        public Project? CurrentProject { get; private set; }

        public event Action<Device>? DeviceStatusChanged;

        public DeviceService(IWakeOnLanService wakeOnLanService, IProgramService programService)
        {
            _wakeOnLanService = wakeOnLanService;
            _programService = programService;
            _wakeOnLanService.HeartbeatReceived += OnHeartbeatReceived;
            _lastHeartbeatTimes = [];
        }

        ~DeviceService()
        {
            if (_offlineCheckTimer != null)
            {
                _offlineCheckTimer.Stop();
                _offlineCheckTimer.Dispose();
                _offlineCheckTimer = null;
            }
        }

        public void WakeAllDevices(Project project)
        {
            if (project == null || project.Devices == null) return;

            foreach (Device device in project.Devices)
            {
                _wakeOnLanService.WakeUpAsync(device.MAC);
            }
        }

        public void ShutdownAllDevices(Project project)
        {
            if (project == null || project.Devices == null) return;

            foreach (Device device in project.Devices)
            {
                _wakeOnLanService.ShutdownAsync(device.IP);
                device.Status = DeviceStatus.Offline;
            }
        }

        public void CheckDeviceStatus(Device device)
        {
            switch (device.Status)
            {
                case DeviceStatus.Online:
                    _wakeOnLanService.ShutdownAsync(device.IP);
                    break;
                case DeviceStatus.Offline:
                    _wakeOnLanService.WakeUpAsync(device.MAC);
                    break;
                case DeviceStatus.Checking:
                    break;
            }

            device.Status = DeviceStatus.Checking;
            DeviceStatusChanged?.Invoke(device);
        }

        private void OnHeartbeatReceived(string ip)
        {
            if (CurrentProject == null || CurrentProject.Devices == null) return;

            foreach (Device device in CurrentProject.Devices)
            {
                if (device.IP == ip)
                {
                    device.Status = DeviceStatus.Online;
                    _lastHeartbeatTimes[device.IP] = DateTime.Now;
                    DeviceStatusChanged?.Invoke(device);
                    break;
                }
            }
        }

        private void CheckDeviceOfflineStatus(object? sender, ElapsedEventArgs e)
        {
            if (CurrentProject == null || CurrentProject.Devices == null) return;

            List<Device> devices = [.. CurrentProject.Devices];

            foreach (Device device in devices)
            {
                // Server PC는 항상 온라인 표시
                if (_programService.IsMyIpAddress(device.IP))
                {
                    device.Status = DeviceStatus.Online;
                }

                if (_lastHeartbeatTimes.TryGetValue(device.IP, out DateTime lastHeartbeatTime))
                {
                    if (device.Status == DeviceStatus.Online && (DateTime.Now - lastHeartbeatTime).TotalMilliseconds > OFFLINE_TIMEOUT_MS)
                    {
                        device.Status = DeviceStatus.Offline;
                    }
                }
                else
                {
                    if (device.Status == DeviceStatus.Online)
                    {
                        device.Status = DeviceStatus.Offline;
                    }
                }

                DeviceStatusChanged?.Invoke(device);
            }
        }

        public void SetCurrentProject(Project project)
        {
            CurrentProject = project;
            _lastHeartbeatTimes.Clear();
            if (_offlineCheckTimer != null)
            {
                _offlineCheckTimer.Stop();
                _offlineCheckTimer.Dispose();
                _offlineCheckTimer = null;
            }
            if (project?.Devices != null)
            {
                foreach (Device device in project.Devices)
                {
                    _lastHeartbeatTimes[device.IP] = DateTime.Now;
                }
            }
            if (_offlineCheckTimer == null)
            {
                _offlineCheckTimer = new System.Timers.Timer(1000);
                _offlineCheckTimer.Elapsed += CheckDeviceOfflineStatus;
                _offlineCheckTimer.AutoReset = true;
                _offlineCheckTimer.Start();
            }
        }
    }
}
