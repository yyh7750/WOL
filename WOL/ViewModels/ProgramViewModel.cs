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

namespace WOL.ViewModels
{
    public class ProgramViewModel : INotifyPropertyChanged
    {
        private readonly IDataService _dataService;
        private readonly IRemoteExplorerService _remoteExplorerService;
        private readonly IProgramService _programService;
        private Project? _currentProject;

        public ObservableCollection<Program> Programs { get; } = [];

        // --- Commands ---
        public ICommand AddProgramCommand { get; }
        public ICommand DeleteProgramCommand { get; }
        public ICommand StartAllProgramsCommand { get; }
        public ICommand StopAllProgramsCommand { get; }
        public ICommand StartProgramCommand { get; }
        public ICommand StopProgramCommand { get; }

        public ProgramViewModel(IDataService dataService, IRemoteExplorerService remoteExplorerService, IProgramService programService)
        {
            _dataService = dataService;
            _remoteExplorerService = remoteExplorerService;
            _programService = programService;

            AddProgramCommand = new RelayCommand<Device>(async (d) => await AddProgramAsync(d!));
            DeleteProgramCommand = new RelayCommand<Program>(async (p) => await DeleteProgramAsync(p!));
            StartAllProgramsCommand = new RelayCommand(async () => await StartAllProgramsAsync(), () => Programs.Any(p => p.Status == ProgramStatus.Stopped));
            StopAllProgramsCommand = new RelayCommand(async () => await StopAllProgramsAsync(), () => Programs.Any(p => p.Status == ProgramStatus.Running));
            StartProgramCommand = new RelayCommand<Device>(d => StartProgramAsync(d!));
            StopProgramCommand = new RelayCommand<Device>(d => StopProgramAsync(d!));
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

        private void StartProgramAsync(Device device)
        {
            _programService.StartProgramAsync(device);
        }

        private void StopProgramAsync(Device device)
        {
            _programService.StopProgramAsync(device);
        }

        private async Task StartAllProgramsAsync()
        {
            await _programService.StartAllProgramsAsync();
        }

        private async Task StopAllProgramsAsync()
        {
            await _programService.StopAllProgramsAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}