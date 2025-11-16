using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RadarProdutos.Domain.Entities;

namespace RadarProdutos.Domain.Interfaces
{
    public interface IProductRepository
    {
        Task AddRangeAsync(IEnumerable<Product> products);
        Task<Product?> GetByIdAsync(Guid id);
        Task<List<Product>> GetPagedAsync(int page, int pageSize, decimal? minMargin, string? competitionLevel, string? sentiment);
        Task<List<Product>> GetAllByAnalysisIdAsync(Guid analysisId);
    }
}
