using WOL.Commands;
using WOL.Models;
using WOL.Services.Interface;
using WOL.View;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WOL.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IDataService _dataService;
        private readonly NewProjectViewModel _newProjectViewModel;
        private Project? _selectedProject;

        // --- 자식 ViewModel ---
        public DeviceViewModel DeviceViewModel { get; }
        public ProgramViewModel ProgramViewModel { get; }

        public ObservableCollection<Project> Projects { get; } = [];

        public Project? SelectedProject
        {
            get => _selectedProject;
            set
            {
                if (SetProperty(ref _selectedProject, value) && value != null)
                {
                    DeviceViewModel.LoadDevicesForProject(value);
                    ProgramViewModel.LoadProgramsForProject(value);
                }
            }
        }

        // --- Commands ---
        public ICommand CreateProjectCommand { get; }
        public ICommand SelectProjectCommand { get; }
        public ICommand UpdateProjectCommand { get; }
        public ICommand DeleteProjectCommand { get; }

        public MainViewModel(IDataService dataService, DeviceViewModel deviceViewModel, ProgramViewModel programViewModel, NewProjectViewModel newProjectViewModel)
        {
            _dataService = dataService;
            _newProjectViewModel = newProjectViewModel;

            DeviceViewModel = deviceViewModel;
            ProgramViewModel = programViewModel;

            CreateProjectCommand = new RelayCommand(async () => await CreateProjectAsync());
            SelectProjectCommand = new RelayCommand<Project>(SelectProject);
            UpdateProjectCommand = new RelayCommand<Project>(async (p) => await UpdateProjectAsync(p), (p) => p != null);
            DeleteProjectCommand = new RelayCommand<Project>(async (p) => await DeleteProjectAsync(p), (p) => p != null);

            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            var projects = await _dataService.ProjectRepository.GetAllProjectsAsync();
            Projects.Clear();
            foreach (var project in projects)
            {
                Projects.Add(project);
            }
            SelectedProject = Projects.FirstOrDefault();
        }

        private void SelectProject(Project? project) => SelectedProject = project;

        private async Task CreateProjectAsync()
        {
            _newProjectViewModel.Initialize(); // 새 프로젝트를 위해 ViewModel 상태 초기화
            var projectView = new NewProjectView
            {
                DataContext = _newProjectViewModel
            };

            if (projectView.ShowDialog() == true)
            {
                var newProject = _newProjectViewModel.Project;
                await _dataService.ProjectRepository.AddProjectAsync(newProject);
                Projects.Add(newProject);
                SelectedProject = newProject;
            }
        }

        private async Task UpdateProjectAsync(Project project)
        {
            _newProjectViewModel.Initialize(project); // 기존 프로젝트 데이터로 ViewModel 상태 설정
            var projectView = new NewProjectView
            {
                DataContext = _newProjectViewModel
            };

            if (projectView.ShowDialog() == true)
            {
                await _dataService.ProjectRepository.UpdateProjectAsync(_newProjectViewModel.Project);
                await LoadDataAsync(); // 목록 새로고침
            }
        }

        private async Task DeleteProjectAsync(Project project)
        {
            await _dataService.ProjectRepository.DeleteProjectAsync(project.Id);
            Projects.Remove(project);
            if (SelectedProject == project)
            {
                SelectedProject = Projects.FirstOrDefault();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
