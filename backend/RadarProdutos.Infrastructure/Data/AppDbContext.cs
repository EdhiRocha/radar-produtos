using Microsoft.EntityFrameworkCore;
using RadarProdutos.Domain.Entities;

namespace RadarProdutos.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<ProductAnalysis> Analyses { get; set; } = null!;
        public DbSet<AnalysisConfig> AnalysisConfigs { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Plan> Plans { get; set; } = null!;
        public DbSet<Subscription> Subscriptions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Product
            modelBuilder.Entity<Product>(b =>
            {
                b.HasKey(p => p.Id);
                b.HasOne(p => p.ProductAnalysis)
                    .WithMany(a => a.Products)
                    .HasForeignKey(p => p.ProductAnalysisId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasIndex(p => p.ExternalId);
                b.HasIndex(p => p.ProductAnalysisId);
            });

            // ProductAnalysis
            modelBuilder.Entity<ProductAnalysis>(b =>
            {
                b.HasKey(a => a.Id);
                b.HasOne(a => a.User)
                    .WithMany(u => u.Analyses)
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.SetNull);

                b.HasIndex(a => a.UserId);
                b.HasIndex(a => a.CreatedAt);
            });

            // AnalysisConfig
            modelBuilder.Entity<AnalysisConfig>(b =>
            {
                b.HasKey(c => c.Id);
            });

            // User
            modelBuilder.Entity<User>(b =>
            {
                b.HasKey(u => u.Id);
                b.HasIndex(u => u.Email).IsUnique();

                b.Property(u => u.Email).HasMaxLength(256).IsRequired();
                b.Property(u => u.Name).HasMaxLength(256).IsRequired();
                b.Property(u => u.PasswordHash).IsRequired();
            });

            // Plan
            modelBuilder.Entity<Plan>(b =>
            {
                b.HasKey(p => p.Id);
                b.Property(p => p.Name).HasMaxLength(50).IsRequired();
                b.Property(p => p.Description).HasMaxLength(500);
                b.Property(p => p.PriceMonthly).HasPrecision(10, 2);

                // Seed data - planos iniciais
                b.HasData(
                    new Plan
                    {
                        Id = 1,
                        Name = "Free",
                        Description = "Plano gratuito com 10 buscas por mês",
                        PriceMonthly = 0,
                        MaxSearchesPerMonth = 10,
                        MaxSearchesPerDay = 3,
                        HasPrioritySupport = false,
                        HasAdvancedFilters = false,
                        IsActive = true
                    },
                    new Plan
                    {
                        Id = 2,
                        Name = "Trial",
                        Description = "Teste grátis de 7 dias com 30 buscas",
                        PriceMonthly = 0,
                        MaxSearchesPerMonth = 30,
                        MaxSearchesPerDay = 10,
                        HasPrioritySupport = false,
                        HasAdvancedFilters = true,
                        IsActive = true
                    },
                    new Plan
                    {
                        Id = 3,
                        Name = "Pro",
                        Description = "Plano profissional com buscas ilimitadas",
                        PriceMonthly = 47.90m,
                        MaxSearchesPerMonth = -1, // ilimitado
                        MaxSearchesPerDay = -1, // ilimitado
                        HasPrioritySupport = true,
                        HasAdvancedFilters = true,
                        IsActive = true
                    }
                );
            });

            // Subscription
            modelBuilder.Entity<Subscription>(b =>
            {
                b.HasKey(s => s.Id);

                b.HasOne(s => s.User)
                    .WithOne(u => u.Subscription)
                    .HasForeignKey<Subscription>(s => s.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(s => s.Plan)
                    .WithMany(p => p.Subscriptions)
                    .HasForeignKey(s => s.PlanId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasIndex(s => s.UserId).IsUnique();
                b.HasIndex(s => new { s.UserId, s.IsActive });
            });
        }
    }
}
