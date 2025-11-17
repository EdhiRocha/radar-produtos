using RadarProdutos.Domain.Entities;

namespace RadarProdutos.Domain.Interfaces;

public interface IShippingEstimateRepository
{
    Task<ShippingEstimate?> GetByCategoryIdAsync(string categoryId);
    Task<List<ShippingEstimate>> GetAllAsync();
    Task<ShippingEstimate> AddAsync(ShippingEstimate estimate);
    Task<ShippingEstimate> UpdateAsync(ShippingEstimate estimate);
}
