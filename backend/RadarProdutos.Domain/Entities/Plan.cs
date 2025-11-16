namespace RadarProdutos.Domain.Entities
{
    public class Plan
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!; // "Free", "Trial", "Pro"
        public string Description { get; set; } = null!;
        public decimal PriceMonthly { get; set; }
        public int MaxSearchesPerMonth { get; set; } // -1 = ilimitado
        public int MaxSearchesPerDay { get; set; } // -1 = ilimitado
        public bool HasPrioritySupport { get; set; }
        public bool HasAdvancedFilters { get; set; }
        public bool IsActive { get; set; } = true;

        // Relacionamento com Subscriptions
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}
