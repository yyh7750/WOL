using WOL.Data;
using WOL.Data.Repositories.Interface;
using WOL.Services.Interface;
using System;
using System.Threading.Tasks;

namespace WOL.Services
{
    public class DataService : IDataService
    {
        private readonly AppDbContext _context;
        public IProjectRepository ProjectRepository { get; }
        public IDeviceRepository DeviceRepository { get; }
        public IProgramRepository ProgramRepository { get; }

        public DataService(AppDbContext context, IProjectRepository projectRepository, IDeviceRepository deviceRepository, IProgramRepository programRepository)
        {
            _context = context;
            ProjectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            DeviceRepository = deviceRepository ?? throw new ArgumentNullException(nameof(deviceRepository));
            ProgramRepository = programRepository ?? throw new ArgumentNullException(nameof(programRepository));
        }
    }
}
