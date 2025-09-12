using Microsoft.EntityFrameworkCore;
using WOL.Data.Repositories.Interface;
using WOL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WOL.Data.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public ProjectRepository(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<Project>> GetAllProjectsAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Project
                                 .Include(p => p.Devices)
                                 .ThenInclude(d => d.Programs)
                                 .ToListAsync();
        }

        public async Task<Project?> GetProjectByIdAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Project
                                 .Include(p => p.Devices)
                                 .ThenInclude(d => d.Programs)
                                 .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddProjectAsync(Project project)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            await context.Project.AddAsync(project);
            await context.SaveChangesAsync();
        }

        public async Task UpdateProjectAsync(Project project)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            context.Project.Update(project);
            await context.SaveChangesAsync();
        }

        public async Task DeleteProjectAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            Project? project = await context.Project.FindAsync(id)
                ?? throw new KeyNotFoundException($"Project with ID {id} not found.");
            
            Device? device = await context.Device.FirstOrDefaultAsync(d => d.ProjectId == id);
            if (device != null)
            {
                Program? program = await context.Program.FirstOrDefaultAsync(p => p.DeviceId == device.Id);

                if (program != null)
                {
                    context.Program.Remove(program);
                }
                context.Device.Remove(device);
            }
            
            context.Project.Remove(project);
            await context.SaveChangesAsync();
        }
    }
}
