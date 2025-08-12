using WOL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WOL.Data.Repositories.Interface
{
    public interface IProgramRepository
    {
        Task<Program?> GetProgramByIdAsync(int id);
        Task<List<Program>> GetAllProgramsAsync();
        Task AddProgramAsync(Program program);
        Task UpdateProgramAsync(Program program);
        Task DeleteProgramAsync(int id);
    }
}
