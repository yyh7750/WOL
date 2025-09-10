using System.Threading.Tasks;
using WOL.Models;

namespace WOL.Services.Interface
{
    public interface IProgramService
    {
        void StartProgramAsync(Device device, Program program);
        void StopProgramAsync(Device device, Program program);
        bool IsMyIpAddress(string ip);
        Task SendProgramSignalAsync(Device device, string runProgramPath, bool isStart);
    }
}
