using System.Collections.Generic;
using System.Threading.Tasks;
using RadarProdutos.Domain.DTOs;

namespace RadarProdutos.Domain.Interfaces
{
    public interface IScraperClient
    {
        Task<List<ScrapedProductDto>> GetProductsFromSupplierAsync(string keyword);
        Task<List<ScrapedProductDto>> GetProductsWithFiltersAsync(string? keyword, string? categoryIds, string? sort, decimal? maxSalePrice, decimal? minSalePrice, int pageNo, int pageSize);
        Task<CompetitionInfoDto?> GetCompetitionInfoAsync(string productName);
        Task<EngagementInfoDto?> GetEngagementInfoAsync(string productName);
    }
}
