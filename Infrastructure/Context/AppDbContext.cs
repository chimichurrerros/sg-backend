using Microsoft.EntityFrameworkCore;
using BackEnd.Models;

namespace BackEnd.Infrastructure.Context;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<PhoneNumber> PhoneNumbers { get; set; }
    public DbSet<Permission> Permissions { get; set; }  

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 0, Name = "Admin" },
            new Role { Id = 1, Name = "User" }
        );
    }
}
