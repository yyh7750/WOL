
using Microsoft.EntityFrameworkCore;
using WOL.Models;

namespace WOL.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Project> Project { get; set; }
        public DbSet<Device> Device { get; set; }
        public DbSet<Program> Program { get; set; }

        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
    }
}
