using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RadarProdutos.Domain.DTOs;
using RadarProdutos.Domain.Interfaces;
using RadarProdutos.Infrastructure.ExternalServices;

namespace RadarProdutos.Infrastructure.Scraper
{
    // Integration layer that uses AliExpress API directly
    public class ScraperHttpClient : IScraperClient
    {
        private readonly IAliExpressClient _aliExpressClient;

        public ScraperHttpClient(IAliExpressClient aliExpressClient)
        {
            _aliExpressClient = aliExpressClient;
        }

        public async Task<List<ScrapedProductDto>> GetProductsFromSupplierAsync(string keyword)
        {
            return await _aliExpressClient.SearchProductsAsync(keyword);
        }

        public async Task<List<ScrapedProductDto>> GetProductsWithFiltersAsync(
            string? keyword,
            string? categoryIds,
            string? sort,
            decimal? maxSalePrice,
            decimal? minSalePrice,
            int pageNo,
            int pageSize)
        {
            return await _aliExpressClient.SearchProductsWithFiltersAsync(
                keyword,
                categoryIds,
                sort,
                maxSalePrice,
                minSalePrice,
                pageNo,
                pageSize);
        }

        public async Task<CompetitionInfoDto?> GetCompetitionInfoAsync(string productName)
        {
            var products = await _aliExpressClient.SearchProductsAsync(productName);

            if (!products.Any())
            {
                return new CompetitionInfoDto
                {
                    TotalCompetitors = 0,
                    AveragePrice = 0,
                    TopSellerCount = 0
                };
            }

            return new CompetitionInfoDto
            {
                TotalCompetitors = products.Count,
                AveragePrice = products.Average(p => p.SupplierPrice),
                TopSellerCount = products.Count(p => p.TotalSales > 1000)
            };
        }

        public async Task<EngagementInfoDto?> GetEngagementInfoAsync(string productName)
        {
            var products = await _aliExpressClient.SearchProductsAsync(productName);

            if (!products.Any())
            {
                return new EngagementInfoDto
                {
                    SearchVolume = 0,
                    SocialMentions = 0,
                    TrendScore = 0
                };
            }

            var totalSales = products.Sum(p => p.TotalSales);
            var avgRating = products.Average(p => p.AverageRating);

            return new EngagementInfoDto
            {
                SearchVolume = totalSales,
                SocialMentions = products.Count * 100,
                TrendScore = avgRating * 20m
            };
        }
    }
}
