using Microsoft.EntityFrameworkCore;
using RickAndMorty.Domain.Models;

namespace RickAndMorty.Infrastructure;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Character> Characters => Set<Character>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Character>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).ValueGeneratedNever();
            
            entity.Property(c => c.Name).IsRequired().HasMaxLength(200);
            entity.Property(c => c.Status).IsRequired().HasMaxLength(50);
            entity.Property(c => c.Species).IsRequired().HasMaxLength(100);
            
            entity.OwnsOne(c => c.Origin, origin =>
            {
                origin.Property(o => o.Name).HasMaxLength(200);
                origin.Property(o => o.Url).HasMaxLength(500);
            });
            
            entity.HasIndex(c => c.Status);
            entity.HasIndex(c => c.CreatedAt);
        });
    }
}