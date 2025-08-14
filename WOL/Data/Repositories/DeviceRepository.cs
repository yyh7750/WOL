using Microsoft.EntityFrameworkCore;
using WOL.Data.Repositories.Interface;
using WOL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WOL.Data.Repositories
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly AppDbContext _context;

        public DeviceRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Device>> GetAllDevicesAsync()
        {
            return await _context.Device
                                 .Include(d => d.Programs)
                                 .ToListAsync();
        }

        public async Task<Device?> GetDeviceByIdAsync(int id)
        {
            return await _context.Device
                                 .Include(d => d.Programs)
                                 .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task AddDeviceAsync(Device device)
        {
            await _context.Device.AddAsync(device);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateDeviceAsync(Device device)
        {
            _context.Device.Update(device);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteDeviceAsync(int id)
        {
            Device? device = await _context.Device.FindAsync(id) ?? throw new KeyNotFoundException($"Device with ID {id} not found.");
            _context.Device.Remove(device);
            await _context.SaveChangesAsync();
        }
    }
}
