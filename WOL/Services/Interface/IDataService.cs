using WOL.Data.Repositories.Interface;
using System.Threading.Tasks;

namespace WOL.Services.Interface
{
    public interface IDataService
    {
        IProjectRepository ProjectRepository { get; }
        IDeviceRepository DeviceRepository { get; }
        IProgramRepository ProgramRepository { get; }
    }
}
