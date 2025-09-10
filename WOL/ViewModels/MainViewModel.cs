using System;
using System.Windows;
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
        private readonly IProgramStatusService _programStatusService;
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

        public MainViewModel(IDataService dataService, DeviceViewModel deviceViewModel, ProgramViewModel programViewModel, NewProjectViewModel newProjectViewModel, IProgramStatusService programStatusService)
        {
            _dataService = dataService;
            _newProjectViewModel = newProjectViewModel;
            _programStatusService = programStatusService;

            DeviceViewModel = deviceViewModel;
            ProgramViewModel = programViewModel;

            CreateProjectCommand = new RelayCommand(async () => await CreateProjectAsync());
            SelectProjectCommand = new RelayCommand<Project>(SelectProject);
            UpdateProjectCommand = new RelayCommand<Project>(async (p) => await UpdateProjectAsync(p), (p) => p != null);
            DeleteProjectCommand = new RelayCommand<Project>(async (p) => await DeleteProjectAsync(p), (p) => p != null);

            _programStatusService.Start();
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                List<Project> projects = await _dataService.ProjectRepository.GetAllProjectsAsync();
                Projects.Clear();
                foreach (Project project in projects)
                {
                    Projects.Add(project);
                }
                SelectedProject = Projects.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"데이터베이스 로딩 중 오류가 발생했습니다. DB 서버가 실행 중인지 확인해주세요.\n\n오류: {ex.Message}", "DB 연결 오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void SelectProject(Project? project) => SelectedProject = project;

        private async Task CreateProjectAsync()
        {
            _newProjectViewModel.Initialize(); // 새 프로젝트를 위해 ViewModel 상태 초기화
            NewProjectView projectView = new()
            {
                DataContext = _newProjectViewModel
            };

            if (projectView.ShowDialog() == true)
            {
                Project newProject = _newProjectViewModel.Project;
                await _dataService.ProjectRepository.AddProjectAsync(newProject);
                Projects.Add(newProject);
                SelectedProject = newProject;
            }
        }

        private async Task UpdateProjectAsync(Project project)
        {
            _newProjectViewModel.Initialize(project); // 기존 프로젝트 데이터로 ViewModel 상태 설정
            NewProjectView projectView = new()
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
