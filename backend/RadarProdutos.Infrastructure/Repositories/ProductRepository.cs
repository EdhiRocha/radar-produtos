using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RadarProdutos.Domain.Entities;
using RadarProdutos.Domain.Interfaces;
using RadarProdutos.Infrastructure.Data;

namespace RadarProdutos.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _db;

        public ProductRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddRangeAsync(IEnumerable<Product> products)
        {
            var productsList = products.ToList();

            // Verifica quais produtos já existem no banco (por ExternalId para evitar duplicatas)
            var externalIds = productsList.Select(p => p.ExternalId).Distinct().ToList();
            var existingExternalIds = await _db.Products
                .Where(p => externalIds.Contains(p.ExternalId))
                .Select(p => p.ExternalId)
                .ToListAsync();

            // Filtra apenas produtos que não existem
            var newProducts = productsList
                .Where(p => !existingExternalIds.Contains(p.ExternalId))
                .ToList();

            if (newProducts.Any())
            {
                await _db.Products.AddRangeAsync(newProducts);
                await _db.SaveChangesAsync();

                Console.WriteLine($"✅ {newProducts.Count} novos produtos salvos no banco");
            }
            else
            {
                Console.WriteLine($"ℹ️ Nenhum produto novo para salvar (todos já existem)");
            }
        }

        public async Task<Product?> GetByIdAsync(Guid id)
        {
            return await _db.Products.Include(p => p.ProductAnalysis).FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Product>> GetPagedAsync(int page, int pageSize, decimal? minMargin, string? competitionLevel, string? sentiment)
        {
            var q = _db.Products.AsQueryable();

            if (minMargin.HasValue)
                q = q.Where(p => p.MarginPercent >= minMargin.Value);

            if (!string.IsNullOrEmpty(competitionLevel))
                q = q.Where(p => p.CompetitionLevel == competitionLevel);

            if (!string.IsNullOrEmpty(sentiment))
                q = q.Where(p => p.Sentiment == sentiment);

            return await q.OrderByDescending(p => p.Score).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<List<Product>> GetAllByAnalysisIdAsync(Guid analysisId)
        {
            return await _db.Products.Where(p => p.ProductAnalysisId == analysisId).OrderByDescending(p => p.Score).ToListAsync();
        }
    }
}
