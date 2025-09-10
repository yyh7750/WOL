using Microsoft.EntityFrameworkCore;
using WOL.Data.Repositories.Interface;
using WOL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WOL.Data.Repositories
{
    public class ProgramRepository : IProgramRepository
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public ProgramRepository(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<Program>> GetAllProgramsAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Program
                                 .ToListAsync();
        }

        public async Task<Program?> GetProgramByIdAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Program
                                 .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddProgramAsync(Program program)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            await context.Program.AddAsync(program);
            await context.SaveChangesAsync();
        }

        public async Task UpdateProgramAsync(Program program)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            context.Program.Update(program);
            await context.SaveChangesAsync();
        }

        public async Task DeleteProgramAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            Program? program = await context.Program.FindAsync(id)
                ?? throw new KeyNotFoundException($"Program with ID {id} not found.");
            context.Program.Remove(program);
            await context.SaveChangesAsync();
        }
    }
}
