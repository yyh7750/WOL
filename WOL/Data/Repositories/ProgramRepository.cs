using Microsoft.EntityFrameworkCore;
using WOL.Data.Repositories.Interface;
using WOL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WOL.Data.Repositories
{
    public class ProgramRepository : IProgramRepository
    {
        private readonly AppDbContext _context;

        public ProgramRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Program>> GetAllProgramsAsync()
        {
            return await _context.Program
                                 .ToListAsync();
        }

        public async Task<Program?> GetProgramByIdAsync(int id)
        {
            return await _context.Program
                                 .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddProgramAsync(Program program)
        {
            await _context.Program.AddAsync(program);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateProgramAsync(Program program)
        {
            _context.Program.Update(program);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProgramAsync(int id)
        {
            Program? program = await _context.Program.FindAsync(id)
                ?? throw new KeyNotFoundException($"Program with ID {id} not found.");
            _context.Program.Remove(program);
            await _context.SaveChangesAsync();
        }
    }
}
