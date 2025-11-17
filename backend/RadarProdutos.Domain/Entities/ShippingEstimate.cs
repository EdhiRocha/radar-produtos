namespace RadarProdutos.Domain.Entities;

public class ShippingEstimate
{
    public int Id { get; set; }
    public string CategoryId { get; set; } = string.Empty; // ID da categoria AliExpress
    public string CategoryName { get; set; } = string.Empty;

    // Estimativas de Peso (kg)
    public decimal MinWeight { get; set; }
    public decimal MaxWeight { get; set; }
    public decimal AverageWeight { get; set; }

    // Custos de Frete (USD)
    public decimal ShippingCostUsd { get; set; }

    // Prazo Estimado (dias)
    public int MinDeliveryDays { get; set; }
    public int MaxDeliveryDays { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
