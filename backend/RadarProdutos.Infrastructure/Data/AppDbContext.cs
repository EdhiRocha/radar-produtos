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
        public DbSet<MarketplaceConfig> MarketplaceConfigs { get; set; } = null!;
        public DbSet<ShippingEstimate> ShippingEstimates { get; set; } = null!;

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

                // Seed data - configuração padrão de análise
                // Pesos ajustados para criar maior diferenciação entre produtos
                b.HasData(new AnalysisConfig
                {
                    Id = 1,
                    MinMarginPercent = 20m,
                    MaxMarginPercent = 80m,
                    WeightSales = 2.5m,         // Vendas têm maior peso (indicador de demanda)
                    WeightCompetition = 2.0m,   // Competição é muito importante
                    WeightSentiment = 1.5m,     // Sentimento impacta conversão
                    WeightMargin = 3.0m         // Margem é crítica para lucratividade
                });
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

            // MarketplaceConfig
            modelBuilder.Entity<MarketplaceConfig>(b =>
            {
                b.HasKey(c => c.Id);

                // Seed data - configuração padrão
                b.HasData(new MarketplaceConfig
                {
                    Id = 1,
                    MinMarginPercent = 30m,
                    TargetMarginPercent = 50m,
                    MercadoLivreFixedFee = 20m,
                    MercadoLivrePercentFee = 15m,
                    MercadoLivreBoostFee = 5m,
                    ImportTaxPercent = 60m,
                    CompanyTaxPercent = 8.93m,
                    UsdToBrlRate = 5.70m,
                    AutoUpdateExchangeRate = false,
                    DefaultShippingCostUsd = 15m,
                    UseEstimatedShipping = true,
                    MinSalesVolume = 100,
                    MinSupplierRating = 4.0m,
                    MaxDeliveryDays = 45,
                    WeightMargin = 1.5m,
                    WeightSales = 1.0m,
                    WeightRating = 1.0m,
                    WeightDelivery = 0.5m,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            });

            // ShippingEstimate
            modelBuilder.Entity<ShippingEstimate>(b =>
            {
                b.HasKey(s => s.Id);
                b.HasIndex(s => s.CategoryId);
            });
        }
    }
}
