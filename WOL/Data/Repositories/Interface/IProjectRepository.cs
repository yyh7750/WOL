
using WOL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WOL.Data.Repositories.Interface
{
    public interface IProjectRepository
    {
        Task<Project?> GetProjectByIdAsync(int id);
        Task<List<Project>> GetAllProjectsAsync();
        Task AddProjectAsync(Project project);
        Task UpdateProjectAsync(Project project);
        Task DeleteProjectAsync(int id);
    }
}
