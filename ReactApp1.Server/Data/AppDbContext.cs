using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ReactApp1.Server.Models.Entities;

namespace ReactApp1.Server.Data
{
    // Matches your existing migration that includes ASP.NET Identity tables.
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Plant> Plants { get; set; }
        public DbSet<Pest> Pests { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Soil> Soils { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(w => w.Ignore(CoreEventId.NavigationBaseIncludeIgnored, CoreEventId.NavigationBaseIncluded));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Plant>(entity =>
            {
                entity.Property(p => p.Name).IsRequired();
                entity.Property(p => p.Description).IsRequired(false);
                entity.Property(p => p.SoilType).IsRequired(false);
                entity.Property(p => p.Pests).IsRequired(false);
                entity.Property(p => p.PestControlMethod).IsRequired(false);
                entity.Property(p => p.ImageUrl).IsRequired(false);
            });

            modelBuilder.Entity<Pest>(entity =>
            {
                entity.Property(p => p.Name).IsRequired();
                entity.Property(p => p.ImageUrl).IsRequired(false);
            });

            modelBuilder.Entity<Comment>(entity =>
            {
                entity.Property(c => c.Email).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Text).IsRequired().HasMaxLength(1000);
            });

            modelBuilder.Entity<Soil>(entity =>
            {
                entity.Property(s => s.Name).IsRequired();
            });
        }
    }
}
