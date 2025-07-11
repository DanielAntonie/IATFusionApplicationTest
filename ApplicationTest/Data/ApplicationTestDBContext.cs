using Microsoft.EntityFrameworkCore;
using System;

namespace ApplicationTest.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Add DbSet properties here later, e.g.
    // public DbSet<User> Users { get; set; }
}
