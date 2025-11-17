namespace RadarProdutos.Domain.Entities;

public class MarketplaceConfig
{
    public int Id { get; set; }

    // Margem e Lucro
    public decimal MinMarginPercent { get; set; } // Ex: 30%
    public decimal TargetMarginPercent { get; set; } // Ex: 50%

    // Taxas Mercado Livre
    public decimal MercadoLivreFixedFee { get; set; } // Ex: R$ 20,00
    public decimal MercadoLivrePercentFee { get; set; } // Ex: 15%
    public decimal MercadoLivreBoostFee { get; set; } // Ex: 5% (impulsionamento)

    // Impostos
    public decimal ImportTaxPercent { get; set; } // Ex: 60%
    public decimal CompanyTaxPercent { get; set; } // Ex: 8.93% (PJ - Simples Nacional)

    // Câmbio
    public decimal UsdToBrlRate { get; set; } // Ex: 5.70
    public bool AutoUpdateExchangeRate { get; set; } // Atualizar taxa automaticamente?

    // Frete
    public decimal DefaultShippingCostUsd { get; set; } // Frete padrão em USD
    public bool UseEstimatedShipping { get; set; } // Usar estimativa por categoria/peso

    // Filtros de Viabilidade
    public int MinSalesVolume { get; set; } // Volume mínimo de vendas no AliExpress
    public decimal MinSupplierRating { get; set; } // Avaliação mínima do fornecedor (0-5)
    public int MaxDeliveryDays { get; set; } // Prazo máximo de entrega

    // Pesos para Score
    public decimal WeightMargin { get; set; } // Peso da margem no score
    public decimal WeightSales { get; set; } // Peso do volume de vendas
    public decimal WeightRating { get; set; } // Peso da avaliação
    public decimal WeightDelivery { get; set; } // Peso do prazo de entrega

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
