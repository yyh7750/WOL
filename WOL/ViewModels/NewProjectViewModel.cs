using WOL.Commands;
using WOL.Models;
using System.Windows;
using System.Windows.Input;

namespace WOL.ViewModels
{
    public class NewProjectViewModel
    {
        public Project Project { get; private set; }

        public NewProjectViewModel()
        {
            Project = new Project(); // Default to a new project
            SetProjectInfoCommand = new RelayCommand<Window>(SetProjectInfo);
        }

        /// <summary>
        /// ViewModel을 재사용하기 위해 Project 객체를 초기화합니다.
        /// </summary>
        /// <param name="project">수정할 프로젝트. 새 프로젝트를 만들려면 null을 전달하세요.</param>
        public void Initialize(Project? project = null)
        {
            Project = project ?? new Project();
        }

        public ICommand SetProjectInfoCommand { get; }

        private void SetProjectInfo(Window? window)
        {
            if (window != null)
            {
                window.DialogResult = true;
                window.Close();
            }
        }
    }
}