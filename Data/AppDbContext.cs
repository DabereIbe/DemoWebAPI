using DemoWebAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DemoWebAPI.Data
{
    public class AppDbContext : IdentityDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }

        public DbSet<Tasks> Tasks { get; set; }
        public new DbSet<Users> Users { get; set; }
        public DbSet<AppUser> AppUsers { get; set; }
    }
}
