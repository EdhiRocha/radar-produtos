using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RadarProdutos.Application.DTOs;
using RadarProdutos.Application.Exceptions;
using RadarProdutos.Application.Requests;
using RadarProdutos.Domain.DTOs;
using RadarProdutos.Domain.Entities;
using RadarProdutos.Domain.Interfaces;

namespace RadarProdutos.Application.Services
{
    public interface IAnalysisService
    {
        Task<List<ProductDto>> RunAnalysisAsync(RunAnalysisRequest request);
        Task<ProductAnalysisDto?> GetLatestAnalysisAsync();
    }

    public class AnalysisService : IAnalysisService
    {
        private readonly IScraperClient _scraper;
        private readonly IProductRepository _productRepository;
        private readonly IProductAnalysisRepository _analysisRepository;
        private readonly IAnalysisConfigRepository _configRepository;
        private readonly ILogger<AnalysisService> _logger;

        public AnalysisService(
            IScraperClient scraper,
            IProductRepository productRepository,
            IProductAnalysisRepository analysisRepository,
            IAnalysisConfigRepository configRepository,
            ILogger<AnalysisService> logger)
        {
            _scraper = scraper;
            _productRepository = productRepository;
            _analysisRepository = analysisRepository;
            _configRepository = configRepository;
            _logger = logger;
        }

        public async Task<List<ProductDto>> RunAnalysisAsync(RunAnalysisRequest request)
        {
            // Chama AliExpress com todos os filtros
            var scraped = await _scraper.GetProductsWithFiltersAsync(
                request.Keyword,
                request.CategoryIds,
                request.Sort,
                request.MaxSalePrice,
                request.MinSalePrice,
                request.PageNo,
                request.PageSize);

            var config = await _configRepository.GetAsync();
            if (config == null)
            {
                _logger.LogError("AnalysisConfig não encontrado no banco de dados");
                throw new ConfigurationNotFoundException("AnalysisConfig");
            }

            var analysis = new ProductAnalysis { Id = Guid.NewGuid(), Keyword = request.Keyword ?? "Radar", CreatedAt = DateTime.UtcNow };

            // Busca agregada de competição/engajamento usando a keyword original
            // Isso faz apenas 2 chamadas extras em vez de 40+
            CompetitionInfoDto? competitionInfo = null;
            EngagementInfoDto? engagementInfo = null;

            try
            {
                // TODO: Mover para background job no futuro
                var competitionTask = _scraper.GetCompetitionInfoAsync(request.Keyword);
                var engagementTask = _scraper.GetEngagementInfoAsync(request.Keyword);

                await Task.WhenAll(competitionTask, engagementTask);

                competitionInfo = await competitionTask;
                engagementInfo = await engagementTask;

                _logger.LogInformation(
                    "Métricas agregadas - Competição: {TotalCompetitors} competidores, Preço médio: {AveragePrice:C2}, Engajamento Volume: {SearchVolume}, TrendScore: {TrendScore}",
                    competitionInfo?.TotalCompetitors,
                    competitionInfo?.AveragePrice,
                    engagementInfo?.SearchVolume,
                    engagementInfo?.TrendScore);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao buscar métricas agregadas. Usando valores padrão");
            }

            var products = new List<Product>();

            foreach (var sp in scraped.Take(50))
            {
                // simple estimation of sale price
                var estimated = sp.SupplierPrice * 1.5m;
                var margin = estimated <= 0 ? 0 : (estimated - sp.SupplierPrice) / estimated * 100m;

                // Determinar competição baseado nos dados agregados + dados do produto
                var competitionLevel = DetermineCompetitionLevel(sp, competitionInfo);

                // Determinar sentimento baseado no rating do produto e engajamento geral
                var sentiment = DetermineSentiment(sp, engagementInfo);

                var product = new Product
                {
                    Id = Guid.NewGuid(),
                    ExternalId = sp.ExternalId,
                    Name = sp.Name,
                    Supplier = sp.Supplier,
                    ImageUrl = sp.ImageUrl,
                    SupplierPrice = sp.SupplierPrice,
                    EstimatedSalePrice = estimated,
                    MarginPercent = decimal.Round(margin, 2),
                    Rating = sp.Rating,
                    Orders = sp.Orders,
                    CompetitionLevel = competitionLevel,
                    Sentiment = sentiment,
                    CreatedAt = DateTime.UtcNow,
                    ProductAnalysisId = analysis.Id
                };

                product.Score = ProductScoreCalculator.CalculateScore(product, config, competitionInfo, engagementInfo);

                products.Add(product);
            }

            analysis.Products.AddRange(products);

            // persist analysis and products
            await _analysisRepository.AddAsync(analysis);
            await _productRepository.AddRangeAsync(products);

            // return DTOs
            return products.Select(p => MapToProductDto(p, scraped.FirstOrDefault(s => s.ExternalId == p.ExternalId))).ToList();
        }

        public async Task<ProductAnalysisDto?> GetLatestAnalysisAsync()
        {
            var analysis = await _analysisRepository.GetLatestAsync();
            if (analysis == null) return null;

            var dto = new ProductAnalysisDto
            {
                Id = analysis.Id,
                Keyword = analysis.Keyword,
                CreatedAt = analysis.CreatedAt,
                Products = analysis.Products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    ExternalId = p.ExternalId,
                    Name = p.Name,
                    Supplier = p.Supplier,
                    ImageUrl = p.ImageUrl,
                    SupplierPrice = p.SupplierPrice,
                    EstimatedSalePrice = p.EstimatedSalePrice,
                    MarginPercent = p.MarginPercent,
                    Rating = p.Rating,
                    Orders = p.Orders,
                    CompetitionLevel = p.CompetitionLevel,
                    Sentiment = p.Sentiment,
                    Score = p.Score
                }).ToList()
            };

            return dto;
        }

        private static string DetermineCompetitionLevel(ScrapedProductDto product, CompetitionInfoDto? competitionInfo)
        {
            // Se não tem dados de competição, usa apenas vendas do produto
            if (competitionInfo == null || competitionInfo.TotalCompetitors == 0)
            {
                return product.Orders switch
                {
                    > 5000 => "Alta",
                    > 1000 => "Media",
                    _ => "Baixa"
                };
            }

            // Com dados de competição: considera número de competidores e top sellers
            var hasHighCompetition = competitionInfo.TotalCompetitors > 15 || competitionInfo.TopSellerCount > 5;
            var hasMediumCompetition = competitionInfo.TotalCompetitors > 8 || competitionInfo.TopSellerCount > 2;

            // Se o produto está acima do preço médio e tem muita competição = Alta
            if (hasHighCompetition && product.SupplierPrice > competitionInfo.AveragePrice)
                return "Alta";

            // Se tem competição média = Media
            if (hasMediumCompetition)
                return "Media";

            return "Baixa";
        }

        private static string DetermineSentiment(ScrapedProductDto product, EngagementInfoDto? engagementInfo)
        {
            // Se não tem dados de engajamento, usa apenas rating do produto
            if (engagementInfo == null || engagementInfo.TrendScore == 0)
            {
                return product.Rating switch
                {
                    >= 4.5m => "Positivo",
                    >= 3.5m => "Misto",
                    _ => "Negativo"
                };
            }

            // Com dados de engajamento: combina rating do produto + trend score geral
            var productRatingScore = product.Rating >= 4.5m ? 2 : product.Rating >= 3.5m ? 1 : 0;
            var trendScoreNormalized = engagementInfo.TrendScore >= 80 ? 2 : engagementInfo.TrendScore >= 60 ? 1 : 0;

            var combinedScore = productRatingScore + trendScoreNormalized;

            return combinedScore switch
            {
                >= 3 => "Positivo",
                >= 2 => "Misto",
                _ => "Negativo"
            };
        }

        private static ProductDto MapToProductDto(Product product, ScrapedProductDto? scrapedData)
        {
            return new ProductDto
            {
                Id = product.Id,
                ExternalId = product.ExternalId,
                Name = product.Name,
                Supplier = product.Supplier,
                ImageUrl = product.ImageUrl,
                SupplierUrl = scrapedData?.SupplierUrl,
                SupplierPrice = product.SupplierPrice,
                EstimatedSalePrice = product.EstimatedSalePrice,
                MarginPercent = product.MarginPercent,
                Rating = product.Rating,
                Orders = product.Orders,
                CompetitionLevel = product.CompetitionLevel,
                Sentiment = product.Sentiment,
                Score = product.Score,

                // Campos extras do ScrapedProductDto
                OriginalPrice = scrapedData?.OriginalPrice,
                Discount = scrapedData?.Discount,
                ShopUrl = scrapedData?.ShopUrl,
                ShopName = scrapedData?.ShopName,
                PromotionLink = scrapedData?.PromotionLink,
                ProductDetailUrl = scrapedData?.ProductDetailUrl,
                FirstLevelCategoryId = scrapedData?.FirstLevelCategoryId,
                FirstLevelCategoryName = scrapedData?.FirstLevelCategoryName,
                ShippingDays = scrapedData?.ShippingDays,
                CommissionRate = scrapedData?.CommissionRate,
                HasVideo = scrapedData?.HasVideo ?? false,
                HasPromotion = !string.IsNullOrEmpty(scrapedData?.PromotionLink)
            };
        }
    }
}
