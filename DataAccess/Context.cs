using Microsoft.EntityFrameworkCore;
using OpenAiBot.Models;

namespace OpenAiBot.DataAccess;

public class Context : DbContext
{
    private readonly ConnectionInfo dbInfo;

    public Context(ConnectionInfo dbInfo)
    {
        this.dbInfo = dbInfo ?? throw new ArgumentNullException(nameof(dbInfo));
    }
    
    public DbSet<User> Users { get; set; } = default!;

    public DbSet<Rule> Rules { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Rule>().HasData(new[]
        {
            // Default rule for all users
            new Rule()
            {
                Id = long.MaxValue,
                MaxRequests = 50,
                MaxTokenProcessed = 2056
            }
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"DataSource={dbInfo.DataSource}");
    }
}