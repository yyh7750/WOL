using WOL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WOL.Data.Repositories.Interface
{
    public interface IDeviceRepository
    {
        Task<Device?> GetDeviceByIdAsync(int id);
        Task<List<Device>> GetAllDevicesAsync();
        Task AddDeviceAsync(Device device);
        Task UpdateDeviceAsync(Device device);
        Task DeleteDeviceAsync(int id);
    }
}
