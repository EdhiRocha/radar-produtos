using Microsoft.EntityFrameworkCore;
using RadarProdutos.Domain.Entities;
using RadarProdutos.Domain.Interfaces;
using RadarProdutos.Infrastructure.Data;

namespace RadarProdutos.Infrastructure.Repositories;

public class MarketplaceConfigRepository : IMarketplaceConfigRepository
{
    private readonly AppDbContext _context;

    public MarketplaceConfigRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<MarketplaceConfig?> GetAsync()
    {
        return await _context.MarketplaceConfigs.FirstOrDefaultAsync();
    }

    public async Task<MarketplaceConfig> UpdateAsync(MarketplaceConfig config)
    {
        config.UpdatedAt = DateTime.UtcNow;
        _context.MarketplaceConfigs.Update(config);
        await _context.SaveChangesAsync();
        return config;
    }
}
