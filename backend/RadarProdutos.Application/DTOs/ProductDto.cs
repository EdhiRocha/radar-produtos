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
        public decimal SupplierPrice { get; set; }
        public decimal EstimatedSalePrice { get; set; }
        public decimal MarginPercent { get; set; }
        public decimal Rating { get; set; }
        public int Orders { get; set; }
        public string CompetitionLevel { get; set; } = "Media";
        public string Sentiment { get; set; } = "Misto";
        public int Score { get; set; }
    }
}
