using RadarProdutos.Domain.Entities;

namespace RadarProdutos.Domain.Interfaces;

public interface IMarketplaceConfigRepository
{
    Task<MarketplaceConfig?> GetAsync();
    Task<MarketplaceConfig> UpdateAsync(MarketplaceConfig config);
}
