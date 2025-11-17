using Microsoft.EntityFrameworkCore;
using RadarProdutos.Domain.Entities;
using RadarProdutos.Domain.Interfaces;
using RadarProdutos.Infrastructure.Data;

namespace RadarProdutos.Infrastructure.Repositories;

public class ShippingEstimateRepository : IShippingEstimateRepository
{
    private readonly AppDbContext _context;

    public ShippingEstimateRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ShippingEstimate?> GetByCategoryIdAsync(string categoryId)
    {
        return await _context.ShippingEstimates
            .FirstOrDefaultAsync(s => s.CategoryId == categoryId);
    }

    public async Task<List<ShippingEstimate>> GetAllAsync()
    {
        return await _context.ShippingEstimates.ToListAsync();
    }

    public async Task<ShippingEstimate> AddAsync(ShippingEstimate estimate)
    {
        estimate.CreatedAt = DateTime.UtcNow;
        estimate.UpdatedAt = DateTime.UtcNow;
        _context.ShippingEstimates.Add(estimate);
        await _context.SaveChangesAsync();
        return estimate;
    }

    public async Task<ShippingEstimate> UpdateAsync(ShippingEstimate estimate)
    {
        estimate.UpdatedAt = DateTime.UtcNow;
        _context.ShippingEstimates.Update(estimate);
        await _context.SaveChangesAsync();
        return estimate;
    }
}
