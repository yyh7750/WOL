using System.Threading.Tasks;
using WOL.Models;

namespace WOL.Services.Interface
{
    public interface IProgramService
    {
        Task StartProgramAsync(Device device);
        Task StopProgramAsync(Device device);
        Task StartAllProgramsAsync();
        Task StopAllProgramsAsync();
        bool IsMyIpAddress(string ip);
    }
}
