using WOL.Commands;
using WOL.Models;
using WOL.Services.Interface;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.Generic;
using System;
using Microsoft.Win32;
using System.Windows;

namespace WOL.ViewModels
{
    public class ProgramViewModel : INotifyPropertyChanged
    {
        private readonly IDataService _dataService;
        private readonly IRemoteExplorerService _remoteExplorerService;
        private readonly IProgramService _programService;
        private readonly IProgramStatusService _programStatusService;
        private Project? _currentProject;

        public ObservableCollection<Program> Programs { get; } = [];

        // --- Commands ---
        public ICommand AddProgramCommand { get; }
        public ICommand DeleteProgramCommand { get; }
        public ICommand StartAllProgramsCommand { get; }
        public ICommand StopAllProgramsCommand { get; }
        public ICommand StartProgramCommand { get; }
        public ICommand StopProgramCommand { get; }

        public ProgramViewModel(IDataService dataService, IRemoteExplorerService remoteExplorerService, IProgramService programService, IProgramStatusService programStatusService)
        {
            _dataService = dataService;
            _remoteExplorerService = remoteExplorerService;
            _programService = programService;
            _programStatusService = programStatusService;
            _programStatusService.ProgramStatusChanged += OnProgramStatusChanged;

            AddProgramCommand = new RelayCommand<Device>(async (d) => await AddProgramAsync(d!));
            DeleteProgramCommand = new RelayCommand<Program>(async (p) => await DeleteProgramAsync(p!));
            StartAllProgramsCommand = new RelayCommand(() => StartAllProgramsAsync());
            StopAllProgramsCommand = new RelayCommand(() => StopAllProgramsAsync());
            StartProgramCommand = new RelayCommand<object>(p => StartProgramAsync(p!));
            StopProgramCommand = new RelayCommand<object>(p => StopProgramAsync(p!));
        }

        public void LoadProgramsForProject(Project project)
        {
            _currentProject = project;
            Programs.Clear();
            if (project?.Devices != null)
            {
                IEnumerable<Program> programs = project.Devices.SelectMany(d => d.Programs);
                foreach (Program program in programs)
                {
                    Programs.Add(program);
                }
            }
        }

        private async Task AddProgramAsync(Device device)
        {
            if (device == null) return;

            if (!_programService.IsMyIpAddress(device.IP)) // Client PC
            {
                Tuple<bool?, List<EntryDto>> dialogResult = await _remoteExplorerService.ShowRemoteExplorerDialogAsync(device);
                bool? result = dialogResult.Item1;
                List<EntryDto> selectedFiles = dialogResult.Item2;

                if (result == true && selectedFiles.Count > 0)
                {
                    foreach (EntryDto f in selectedFiles)
                    {
                        Program newProgram = new()
                        {
                            Name = System.IO.Path.GetFileNameWithoutExtension(f.FullPath),
                            Path = f.FullPath,
                            Status = ProgramStatus.Stopped,
                            DeviceId = device.Id
                        };
                        
                        await _dataService.ProgramRepository.AddProgramAsync(newProgram);
                        Programs.Add(newProgram);
                    }
                }
            }
            else // Server(Local) PC
            {
                OpenFileDialog openFileDialog = new() { Multiselect = true, Filter = "All files (*.*)|*.*" };
                if (openFileDialog.ShowDialog() == true)
                {
                    foreach (string filename in openFileDialog.FileNames)
                    {
                        Program newProgram = new()
                        {
                            Name = System.IO.Path.GetFileNameWithoutExtension(filename),
                            Path = filename,
                            Status = ProgramStatus.Stopped,
                            DeviceId = device.Id
                        };
                        
                        await _dataService.ProgramRepository.AddProgramAsync(newProgram);
                        Programs.Add(newProgram);
                    }
                }
            }
        }

        private async Task DeleteProgramAsync(Program program)
        {
            if (program == null) return;
            await _dataService.ProgramRepository.DeleteProgramAsync(program.Id);
            Programs.Remove(program);
        }

        private void StartProgramAsync(object parameter)
        {
            if (parameter is object[] values && values.Length == 2)
            {
                if (values[0] is Device device && values[1] is Program program)
                {
                    _programService.StartProgramAsync(device, program);
                }
            }
        }

        private void StopProgramAsync(object parameter)
        {
            if (parameter is object[] values && values.Length == 2)
            {
                if (values[0] is Device device && values[1] is Program program)
                {
                    _programService.StopProgramAsync(device, program);
                }
            }
        }

        private void StartAllProgramsAsync()
        {
            if (_currentProject == null) return;

            foreach (Device device in _currentProject.Devices)
            {
                foreach (Program program in device.Programs)
                {
                    if (program.Status == ProgramStatus.Stopped)
                    {
                        _programService.StartProgramAsync(device, program);
                    }
                }
            }
        }

        private void StopAllProgramsAsync()
        {
            if (_currentProject == null) return;
            
            foreach (Device device in _currentProject.Devices)
            {
                foreach (Program program in device.Programs)
                {
                    if (program.Status == ProgramStatus.Running)
                    {
                        _programService.StopProgramAsync(device, program);
                    }
                }
            }
        }

        private void OnProgramStatusChanged(int programId, ProgramStatus newStatus)
        {
            var program = Programs.FirstOrDefault(p => p.Id == programId);
            if (program != null && program.Status != newStatus)
            {
                Application.Current.Dispatcher.Invoke(() => program.Status = newStatus);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}