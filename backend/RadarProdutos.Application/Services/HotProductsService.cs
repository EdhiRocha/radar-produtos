using Microsoft.Extensions.Logging;
using RadarProdutos.Application.DTOs;
using RadarProdutos.Application.Exceptions;
using RadarProdutos.Application.Mappers;
using RadarProdutos.Application.Requests;
using RadarProdutos.Domain.Entities;
using RadarProdutos.Domain.Interfaces;
using RadarProdutos.Infrastructure.ExternalServices;

namespace RadarProdutos.Application.Services;

public interface IHotProductsService
{
    Task<IReadOnlyList<ProductDto>> GetHotProductsAsync(HotProductsFilterDto filter, CancellationToken cancellationToken = default);
}

public class HotProductsService : IHotProductsService
{
    private readonly IAliExpressClient _aliClient;
    private readonly IAnalysisConfigRepository _configRepository;
    private readonly IMarketplaceConfigRepository _marketplaceConfigRepository;
    private readonly ILogger<HotProductsService> _logger;

    public HotProductsService(
        IAliExpressClient aliClient,
        IAnalysisConfigRepository configRepository,
        IMarketplaceConfigRepository marketplaceConfigRepository,
        ILogger<HotProductsService> logger)
    {
        _aliClient = aliClient;
        _configRepository = configRepository;
        _marketplaceConfigRepository = marketplaceConfigRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ProductDto>> GetHotProductsAsync(HotProductsFilterDto filter, CancellationToken cancellationToken = default)
    {
        var raw = await _aliClient.GetHotProductsAsync(
            filter.Keyword,
            filter.CategoryIds,
            filter.MinSalePrice,
            filter.MaxSalePrice,
            filter.PageNo,
            filter.PageSize,
            filter.Sort,
            filter.PlatformProductType
        );

        if (raw?.RespResult?.Result?.Products == null || !raw.RespResult.Result.Products.Any())
        {
            _logger.LogWarning("Nenhum Hot Product retornado pela API para keyword: {Keyword}", filter.Keyword);
            return Array.Empty<ProductDto>();
        }

        var config = await _configRepository.GetAsync();
        if (config == null)
        {
            _logger.LogError("AnalysisConfig não encontrado no banco de dados");
            throw new ConfigurationNotFoundException("AnalysisConfig");
        }

        var marketplaceConfig = await _marketplaceConfigRepository.GetAsync();
        if (marketplaceConfig == null)
        {
            _logger.LogError("MarketplaceConfig não encontrado no banco de dados");
            throw new ConfigurationNotFoundException("MarketplaceConfig");
        }

        // Mapear para ProductDto e filtrar produtos de baixa qualidade
        var list = raw.RespResult.Result.Products
            .Select(HotProductMapper.ToProductDto)
            .Where(p =>
                p.Rating >= 3.5m &&           // Rating mínimo
                p.SupplierPrice > 0 &&        // Deve ter preço válido
                !string.IsNullOrEmpty(p.ImageUrl) // Deve ter imagem
            )
            .ToList();

        if (list.Count == 0)
        {
            _logger.LogWarning("Nenhum produto passou nos filtros de qualidade mínima após mapeamento");
        }

        // Calcula score e viabilidade para cada produto
        foreach (var product in list)
        {
            // Usa novo método que considera métricas adicionais (shipping, comissão, vídeo)
            product.Score = ProductScoreCalculator.CalculateScore(product, config);

            // Calcular viabilidade se config existe
            if (marketplaceConfig != null)
            {
                var viability = ProductViabilityCalculator.Calculate(
                    product.SupplierPrice,
                    marketplaceConfig.DefaultShippingCostUsd,
                    product.Orders,
                    product.Rating,
                    marketplaceConfig
                );

                product.Viability = new ProductViabilityDto
                {
                    TotalAcquisitionCost = viability.TotalAcquisitionCost,
                    SuggestedSalePrice = viability.SuggestedSalePrice,
                    NetProfit = viability.NetProfit,
                    RealMarginPercent = viability.RealMarginPercent,
                    ROI = viability.ROI,
                    IsViable = viability.IsViable,
                    ViabilityScore = viability.ViabilityScore,
                    ProductPriceBrl = viability.ProductPriceBrl,
                    ShippingCostBrl = viability.ShippingCostBrl,
                    ImportTax = viability.ImportTax,
                    TotalMercadoLivreFees = viability.TotalMercadoLivreFees
                };

                // Atualizar score combinando score original com viability score
                product.Score = (int)((product.Score + viability.ViabilityScore) / 2);
            }
        }

        // Ordena por viabilidade (viáveis primeiro) e depois por score
        return list
            .OrderByDescending(p => p.Viability?.IsViable ?? false)
            .ThenByDescending(p => p.Score)
            .ToList();
    }
}
