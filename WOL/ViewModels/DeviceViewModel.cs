using WOL.Commands;
using WOL.Models;
using WOL.Services.Interface;
using WOL.View;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System;
using System.Windows;

namespace WOL.ViewModels
{
    public class DeviceViewModel : INotifyPropertyChanged
    {
        private readonly IDataService _dataService;
        private readonly NewDeviceViewModel _newDeviceViewModel;
        private readonly IDeviceService _deviceService;
        private Project? _currentProject;

        public ObservableCollection<Device> Devices { get; } = [];

        // Commands
        public ICommand AddDeviceCommand { get; }
        public ICommand UpdateDeviceCommand { get; }
        public ICommand DeleteDeviceCommand { get; }
        public ICommand WakeAllDevicesCommand { get; }
        public ICommand ShutdownAllDevicesCommand { get; }

        public DeviceViewModel(IDataService dataService, NewDeviceViewModel newDeviceViewModel, IDeviceService deviceService)
        {
            _dataService = dataService;
            _newDeviceViewModel = newDeviceViewModel;
            _deviceService = deviceService;
            _deviceService.DeviceStatusChanged += OnDeviceStatusChanged;

            AddDeviceCommand = new RelayCommand(async () => await AddDeviceAsync(), () => _currentProject != null);
            UpdateDeviceCommand = new RelayCommand<Device>(async (d) => await UpdateDeviceAsync(d), (d) => d != null);
            DeleteDeviceCommand = new RelayCommand<Device>(async (d) => await DeleteDeviceAsync(d), (d) => d != null);
            WakeAllDevicesCommand = new RelayCommand(async () => await WakeAllDevices(), () => _currentProject != null);
            ShutdownAllDevicesCommand = new RelayCommand(async () => await ShutdownAllDevices(), () => _currentProject != null);
        }

        public void LoadDevicesForProject(Project project)
        {
            ArgumentNullException.ThrowIfNull(project);

            _currentProject = project;
            Devices.Clear();
            if (project?.Devices != null)
            {
                foreach (var device in project.Devices)
                {
                    Devices.Add(device);
                }
            }

            _deviceService.SetCurrentProject(_currentProject);
        }

        private async Task AddDeviceAsync()
        {
            if (_currentProject == null) return;

            _newDeviceViewModel.Initialize(); // 새 디바이스를 위해 ViewModel 상태 초기화
            var deviceView = new NewDeviceView
            {
                DataContext = _newDeviceViewModel
            };

            if (deviceView.ShowDialog() == true)
            {
                var newDevice = _newDeviceViewModel.Device;
                newDevice.ProjectId = _currentProject.Id;
                await _dataService.DeviceRepository.AddDeviceAsync(newDevice);
                Devices.Add(newDevice);
            }
        }

        private async Task UpdateDeviceAsync(Device device)
        {
            if (_currentProject == null) return;

            _newDeviceViewModel.Initialize(device); // 기존 디바이스 데이터로 ViewModel 상태 설정
            var deviceView = new NewDeviceView
            {
                DataContext = _newDeviceViewModel
            };

            if (deviceView.ShowDialog() == true)
            {
                await _dataService.DeviceRepository.UpdateDeviceAsync(_newDeviceViewModel.Device);
                LoadDevicesForProject(_currentProject);
            }
        }

        private async Task DeleteDeviceAsync(Device device)
        {
            await _dataService.DeviceRepository.DeleteDeviceAsync(device.Id);
            Devices.Remove(device);
        }

        private async Task WakeAllDevices()
        {
            if (_currentProject == null) return;
            await Task.Run(() => _deviceService.WakeAllDevices(_currentProject));
        }

        private async Task ShutdownAllDevices()
        {
            if (_currentProject == null) return;
            await Task.Run(() => _deviceService.ShutdownAllDevices(_currentProject));
        }

        private void OnDeviceStatusChanged(Device device)
        {
            if (_currentProject == null || device.ProjectId != _currentProject.Id) return;

            if (_currentProject.Devices.Contains(device))
            {
                _currentProject.Devices[_currentProject.Devices.IndexOf(device)] = device;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    LoadDevicesForProject(_currentProject);
                });
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
