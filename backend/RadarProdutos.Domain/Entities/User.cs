namespace RadarProdutos.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Name { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Relacionamento com Subscription
        public Subscription? Subscription { get; set; }

        // Relacionamento com ProductAnalysis
        public ICollection<ProductAnalysis> Analyses { get; set; } = new List<ProductAnalysis>();
    }
}
