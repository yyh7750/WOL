using WOL.Data.Repositories.Interface;
using WOL.Services.Interface;
using System;

namespace WOL.Services
{
    public class DataService : IDataService
    {
        public IProjectRepository ProjectRepository { get; }
        public IDeviceRepository DeviceRepository { get; }
        public IProgramRepository ProgramRepository { get; }

        public DataService(IProjectRepository projectRepository, IDeviceRepository deviceRepository, IProgramRepository programRepository)
        {
            ProjectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            DeviceRepository = deviceRepository ?? throw new ArgumentNullException(nameof(deviceRepository));
            ProgramRepository = programRepository ?? throw new ArgumentNullException(nameof(programRepository));
        }
    }
}
