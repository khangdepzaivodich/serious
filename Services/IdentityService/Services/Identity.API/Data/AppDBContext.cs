using IdentityService.Services.Identity.API.Models;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Services.Identity.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
    }
}