using glms.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace glms.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }
        public DbSet<glms.Models.Entities.User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ServiceRequest>(entity =>
            {
                entity.Property(s => s.Cost)
                      .HasPrecision(18, 2);

                entity.Property(s => s.ConvertedCost)
                      .HasPrecision(18, 2);
            });
        }
    }
}