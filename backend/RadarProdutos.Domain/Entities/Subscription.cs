namespace RadarProdutos.Domain.Entities
{
    public class Subscription
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public int PlanId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; } = true;

        // Controle de uso
        public int SearchesUsedThisMonth { get; set; }
        public int SearchesUsedToday { get; set; }
        public DateTime LastResetDate { get; set; }
        public DateTime LastSearchDate { get; set; }

        // Relacionamentos
        public User User { get; set; } = null!;
        public Plan Plan { get; set; } = null!;
    }
}
