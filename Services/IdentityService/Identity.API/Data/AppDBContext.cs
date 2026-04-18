using backend.Services.Identity.API.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services.Identity.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
    }
}