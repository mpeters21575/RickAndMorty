using Microsoft.EntityFrameworkCore;
using RickAndMorty.Domain.Models;

namespace RickAndMorty.Infrastructure;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Character> Characters => Set<Character>();
    public DbSet<Episode> Episodes => Set<Episode>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Character>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).ValueGeneratedNever();
            
            entity.Property(c => c.Name).IsRequired().HasMaxLength(200);
            entity.Property(c => c.Status).IsRequired().HasMaxLength(50);
            entity.Property(c => c.Species).IsRequired().HasMaxLength(100);
            entity.Property(c => c.ImageUrl).IsRequired().HasMaxLength(500);
            
            entity.OwnsOne(c => c.Origin, origin =>
            {
                origin.Property(o => o.Name).HasMaxLength(200);
                origin.Property(o => o.Url).HasMaxLength(500);
            });
            
            entity.HasIndex(c => c.Status);
            entity.HasIndex(c => c.CreatedAt);
        });

        modelBuilder.Entity<Episode>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.EpisodeCode).IsRequired().HasMaxLength(10);
            entity.Property(e => e.AirDate).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Url).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Characters).HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
            );
            
            entity.HasIndex(e => e.EpisodeCode);
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}