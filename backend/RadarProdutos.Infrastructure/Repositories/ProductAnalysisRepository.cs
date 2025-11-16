using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RadarProdutos.Domain.Entities;
using RadarProdutos.Domain.Interfaces;
using RadarProdutos.Infrastructure.Data;

namespace RadarProdutos.Infrastructure.Repositories
{
    public class ProductAnalysisRepository : IProductAnalysisRepository
    {
        private readonly AppDbContext _db;

        public ProductAnalysisRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(ProductAnalysis analysis)
        {
            await _db.Analyses.AddAsync(analysis);
            await _db.SaveChangesAsync();
        }

        public async Task<ProductAnalysis?> GetByIdAsync(Guid id)
        {
            return await _db.Analyses
                .Include(a => a.Products)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<ProductAnalysis?> GetLatestAsync()
        {
            return await _db.Analyses
                .Include(a => a.Products)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync();
        }
    }
}
