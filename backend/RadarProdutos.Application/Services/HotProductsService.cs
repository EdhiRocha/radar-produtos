using RadarProdutos.Application.DTOs;
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

    public HotProductsService(IAliExpressClient aliClient, IAnalysisConfigRepository configRepository)
    {
        _aliClient = aliClient;
        _configRepository = configRepository;
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
            Console.WriteLine("⚠️ Nenhum Hot Product retornado pela API");
            return Array.Empty<ProductDto>();
        }

        var config = await _configRepository.GetAsync() ?? new AnalysisConfig
        {
            Id = 1,
            MinMarginPercent = 10,
            MaxMarginPercent = 60,
            WeightSales = 1,
            WeightCompetition = 1,
            WeightSentiment = 1,
            WeightMargin = 1
        };

        var list = raw.RespResult.Result.Products
            .Select(HotProductMapper.ToProductDto)
            .ToList();

        // Calcula score para cada produto
        foreach (var product in list)
        {
            // Criar um Product temporário para calcular o score
            var tempProduct = new Domain.Entities.Product
            {
                MarginPercent = product.MarginPercent,
                Rating = product.Rating,
                Orders = product.Orders,
                CompetitionLevel = product.CompetitionLevel,
                Sentiment = product.Sentiment
            };

            product.Score = ProductScoreCalculator.CalculateScore(tempProduct, config, null, null);
        }

        // Ordena por score decrescente
        return list.OrderByDescending(p => p.Score).ToList();
    }
}
