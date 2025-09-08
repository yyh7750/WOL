using System.Threading.Tasks;
using WOL.Models;

namespace WOL.Services.Interface
{
    public interface IProgramService
    {
        void StartProgramAsync(Device device);
        void StopProgramAsync(Device device);
        Task StartAllProgramsAsync();
        Task StopAllProgramsAsync();
        bool IsMyIpAddress(string ip);
        Task SendProgramSignalAsync(Device device, bool isStart);
    }
}