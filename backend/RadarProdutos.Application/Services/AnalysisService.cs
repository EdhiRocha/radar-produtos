using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RadarProdutos.Application.DTOs;
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

        public AnalysisService(
            IScraperClient scraper,
            IProductRepository productRepository,
            IProductAnalysisRepository analysisRepository,
            IAnalysisConfigRepository configRepository)
        {
            _scraper = scraper;
            _productRepository = productRepository;
            _analysisRepository = analysisRepository;
            _configRepository = configRepository;
        }

        public async Task<List<ProductDto>> RunAnalysisAsync(RunAnalysisRequest request)
        {
            var scraped = await _scraper.GetProductsFromSupplierAsync(request.Keyword);

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

            var analysis = new ProductAnalysis { Id = Guid.NewGuid(), Keyword = request.Keyword, CreatedAt = DateTime.UtcNow };

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

                Console.WriteLine($"📊 Competição: {competitionInfo?.TotalCompetitors} competidores, preço médio: {competitionInfo?.AveragePrice:C2}");
                Console.WriteLine($"📈 Engajamento: Volume {engagementInfo?.SearchVolume}, TrendScore: {engagementInfo?.TrendScore}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Erro ao buscar métricas agregadas: {ex.Message}. Usando valores padrão.");
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
            return products.Select(p => new ProductDto
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
            }).ToList();
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
    }
}
