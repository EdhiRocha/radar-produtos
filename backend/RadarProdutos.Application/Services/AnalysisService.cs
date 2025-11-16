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

            var products = new List<Product>();

            foreach (var sp in scraped.Take(50))
            {
                var competition = await _scraper.GetCompetitionInfoAsync(sp.Name);
                var engagement = await _scraper.GetEngagementInfoAsync(sp.Name);

                // simple estimation of sale price
                var estimated = sp.SupplierPrice * 1.5m;
                var margin = estimated <= 0 ? 0 : (estimated - sp.SupplierPrice) / estimated * 100m;

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
                    CompetitionLevel = competition?.CompetitionLevel ?? "Media",
                    Sentiment = engagement?.Sentiment ?? "Misto",
                    CreatedAt = DateTime.UtcNow,
                    ProductAnalysisId = analysis.Id
                };

                product.Score = ProductScoreCalculator.CalculateScore(product, config, competition, engagement);

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
    }
}
