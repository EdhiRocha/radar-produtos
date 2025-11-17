using System;

namespace RadarProdutos.Application.DTOs
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string ExternalId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Supplier { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public string? SupplierUrl { get; set; }
        public decimal SupplierPrice { get; set; }
        public decimal EstimatedSalePrice { get; set; }
        public decimal MarginPercent { get; set; }
        public decimal Rating { get; set; }
        public int Orders { get; set; }
        public string CompetitionLevel { get; set; } = "Media";
        public string Sentiment { get; set; } = "Misto";
        public int Score { get; set; }

        // Métricas adicionais da API para melhor análise
        public string? ShopName { get; set; }
        public string? ShopUrl { get; set; }
        public int? ShippingDays { get; set; }
        public decimal? CommissionRate { get; set; }
        public bool HasVideo { get; set; }
        public bool HasPromotion { get; set; }

        // Novos campos importantes
        public decimal? OriginalPrice { get; set; }
        public string? Discount { get; set; } // Ex: "50%"
        public string? PromotionLink { get; set; }
        public string? ProductDetailUrl { get; set; }
        public string? FirstLevelCategoryId { get; set; }
        public string? FirstLevelCategoryName { get; set; }

        // Dados de Viabilidade
        public ProductViabilityDto? Viability { get; set; }
    }

    public class ProductViabilityDto
    {
        public decimal TotalAcquisitionCost { get; set; }
        public decimal SuggestedSalePrice { get; set; }
        public decimal NetProfit { get; set; }
        public decimal RealMarginPercent { get; set; }
        public decimal ROI { get; set; }
        public bool IsViable { get; set; }
        public decimal ViabilityScore { get; set; }

        // Detalhamento de Custos
        public decimal ProductPriceBrl { get; set; }
        public decimal ShippingCostBrl { get; set; }
        public decimal ImportTax { get; set; }
        public decimal TotalMercadoLivreFees { get; set; }
    }

    public class PaginatedResponse<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}
