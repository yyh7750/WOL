using System;
using System.Threading.Tasks;
using WOL.Models;

namespace WOL.Services.Interface
{
    public interface IProgramStatusService
    {
        event Action<int, ProgramStatus> ProgramStatusChanged;

        void Start();
        void Stop();
        Task<bool> IsLocalProgramRunningAsync(Program program);
        Task<bool> IsRemoteProgramRunningAsync(Device device, Program program);
    }
}
