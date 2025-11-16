using System;

namespace RadarProdutos.Domain.Entities
{
    // Representa um produto analisado
    public class Product
    {
        public Guid Id { get; set; }
        public string ExternalId { get; set; } = null!; // id do AliExpress ou similar
        public string Name { get; set; } = null!;
        public string Supplier { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public decimal SupplierPrice { get; set; }
        public decimal EstimatedSalePrice { get; set; }
        public decimal MarginPercent { get; set; }
        public decimal Rating { get; set; }
        public int Orders { get; set; }
        public string CompetitionLevel { get; set; } = "Media"; // Baixa, Media, Alta
        public string Sentiment { get; set; } = "Misto"; // Positivo, Negativo, Misto
        public int Score { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Relationship
        public Guid? ProductAnalysisId { get; set; }
        public ProductAnalysis? ProductAnalysis { get; set; }
    }
}
