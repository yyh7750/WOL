using Microsoft.EntityFrameworkCore;
using WOL.Data.Repositories.Interface;
using WOL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WOL.Data.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly AppDbContext _context;

        public ProjectRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Project>> GetAllProjectsAsync()
        {
            return await _context.Project
                                 .Include(p => p.Devices)
                                 .ThenInclude(d => d.Programs)
                                 .ToListAsync();
        }

        public async Task<Project?> GetProjectByIdAsync(int id)
        {
            return await _context.Project
                                 .Include(p => p.Devices)
                                 .ThenInclude(d => d.Programs)
                                 .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddProjectAsync(Project project)
        {
            await _context.Project.AddAsync(project);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateProjectAsync(Project project)
        {
            _context.Project.Update(project);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProjectAsync(int id)
        {
            Project? project = await _context.Project.FindAsync(id) 
                ?? throw new KeyNotFoundException($"Project with ID {id} not found.");
            _context.Project.Remove(project);
            await _context.SaveChangesAsync();
        }
    }
}
