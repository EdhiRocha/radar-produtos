namespace RadarProdutos.Domain.Entities
{
    // Representa uma execução de análise (aggregates produtos analisados)
    public class ProductAnalysis
    {
        public Guid Id { get; set; }
        public string Keyword { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        // Multi-tenant
        public Guid? UserId { get; set; }
        public User? User { get; set; }

        // Navigation
        public List<Product> Products { get; set; } = new List<Product>();
    }
}
