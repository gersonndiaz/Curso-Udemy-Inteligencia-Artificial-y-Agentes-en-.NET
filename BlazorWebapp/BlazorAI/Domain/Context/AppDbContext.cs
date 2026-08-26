using BlazorAI.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorAI.Domain.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options) : base(options)
    {}

    protected AppDbContext() {}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Person>().HasData(
            new Person
            {
                Id = 1,
                Name = "Felipe Gavilán",
                Email = "felipe.gavilan@example.com",
                Salary = 45000m,
                Active = true
            },
            new Person
            {
                Id = 2,
                Name = "María López",
                Email = "maria.lopez@example.com",
                Salary = 52000m,
                Active = true
            },
            new Person
            {
                Id = 3,
                Name = "Carlos Rodríguez",
                Email = "carlos.rodriguez@example.com",
                Salary = 61000m,
                Active = false
            },
            new Person
            {
                Id = 4,
                Name = "Ana Martínez",
                Email = "ana.martinez@example.com",
                Salary = 48000m,
                Active = false
            },
            new Person
            {
                Id = 5,
                Name = "Luis Gómez",
                Email = "luis.gomez@example.com",
                Salary = 55000m,
                Active = true
            }
        );
    }

    public DbSet<Person> People { get; set; }
}
