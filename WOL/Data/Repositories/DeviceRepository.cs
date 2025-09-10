using Microsoft.EntityFrameworkCore;
using WOL.Data.Repositories.Interface;
using WOL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WOL.Data.Repositories
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public DeviceRepository(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<Device>> GetAllDevicesAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Device
                                 .Include(d => d.Programs)
                                 .ToListAsync();
        }

        public async Task<Device?> GetDeviceByIdAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Device
                                 .Include(d => d.Programs)
                                 .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task AddDeviceAsync(Device device)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            await context.Device.AddAsync(device);
            await context.SaveChangesAsync();
        }

        public async Task UpdateDeviceAsync(Device device)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            context.Device.Update(device);
            await context.SaveChangesAsync();
        }

        public async Task DeleteDeviceAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            Device? device = await context.Device.FindAsync(id) ?? throw new KeyNotFoundException($"Device with ID {id} not found.");
            context.Device.Remove(device);
            await context.SaveChangesAsync();
        }
    }
}
