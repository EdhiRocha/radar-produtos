namespace RadarProdutos.Domain.DTOs
{
    // DTOs used between the scraper microservice and the domain/application
    public class ScrapedProductDto
    {
        public string ExternalId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Supplier { get; set; } = "AliExpress";
        public string? ImageUrl { get; set; }
        public string? SupplierUrl { get; set; }
        public decimal SupplierPrice { get; set; }
        public decimal Rating { get; set; }
        public decimal AverageRating { get; set; }
        public int Orders { get; set; }
        public int TotalSales { get; set; }
    }

    public class CompetitionInfoDto
    {
        public string CompetitionLevel { get; set; } = "Media"; // Baixa|Media|Alta
        public int TotalCompetitors { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public int TopSellerCount { get; set; }
    }

    public class EngagementInfoDto
    {
        public string Sentiment { get; set; } = "Misto"; // Positivo|Negativo|Misto
        public int SearchVolume { get; set; }
        public int SocialMentions { get; set; }
        public decimal TrendScore { get; set; }
        public decimal YoutubeEngagementPercent { get; set; }
    }
}
