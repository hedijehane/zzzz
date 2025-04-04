// Intranet.Infrastructure/Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using Intranet.Domain;

namespace Intranet.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; }
}

    