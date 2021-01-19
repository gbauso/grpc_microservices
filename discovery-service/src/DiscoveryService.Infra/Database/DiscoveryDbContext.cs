using DiscoveryService.Infra.Model;
using Microsoft.EntityFrameworkCore;

namespace DiscoveryService.Infra.Database
{
    public class DiscoveryDbContext : DbContext
    {
        public DiscoveryDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Service> Services { get; set; }
        public DbSet<GrpcMethod> GrpcMethods { get; set; }
        public DbSet<ServiceMethod> ServiceMethods { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Service>().UseXminAsConcurrencyToken();
            modelBuilder.Entity<GrpcMethod>().UseXminAsConcurrencyToken();
            modelBuilder.Entity<ServiceMethod>().UseXminAsConcurrencyToken();

            modelBuilder.HasPostgresExtension("uuid-ossp")
                   .Entity<Service>()
                   .Property(e => e.Id)
                   .HasDefaultValueSql("uuid_generate_v4()");

            modelBuilder.HasPostgresExtension("uuid-ossp")
                   .Entity<GrpcMethod>()
                   .Property(e => e.Id)
                   .HasDefaultValueSql("uuid_generate_v4()");

            modelBuilder.HasPostgresExtension("uuid-ossp")
                   .Entity<ServiceMethod>()
                   .Property(e => e.Id)
                   .HasDefaultValueSql("uuid_generate_v4()");
        }
    }
}
