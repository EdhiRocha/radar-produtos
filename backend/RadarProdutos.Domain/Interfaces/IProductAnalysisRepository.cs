using System;
using System.Threading.Tasks;
using RadarProdutos.Domain.Entities;

namespace RadarProdutos.Domain.Interfaces
{
    public interface IProductAnalysisRepository
    {
        Task AddAsync(ProductAnalysis analysis);
        Task<ProductAnalysis?> GetByIdAsync(Guid id);
        Task<ProductAnalysis?> GetLatestAsync();
    }
}
