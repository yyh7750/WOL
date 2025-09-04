using System.Threading.Tasks;

namespace WOL.Services.Interface
{
    public interface IProgramService
    {
        Task StartProgramAsync();
        Task StopProgramAsync();
        Task StartAllProgramsAsync();
        Task StopAllProgramsAsync();
        bool IsMyIpAddress(string ip);
    }
}
