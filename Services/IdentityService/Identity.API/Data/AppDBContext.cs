using IdentityService.Identity.API.Models;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Identity.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
    }
}