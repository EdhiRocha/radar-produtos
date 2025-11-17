namespace RadarProdutos.Application.DTOs;

public class MarketplaceConfigDto
{
    public int Id { get; set; }

    // Margem e Lucro
    public decimal MinMarginPercent { get; set; }
    public decimal TargetMarginPercent { get; set; }

    // Taxas Mercado Livre
    public decimal MercadoLivreFixedFee { get; set; }
    public decimal MercadoLivrePercentFee { get; set; }
    public decimal MercadoLivreBoostFee { get; set; }

    // Impostos
    public decimal ImportTaxPercent { get; set; }
    public decimal CompanyTaxPercent { get; set; }

    // Câmbio
    public decimal UsdToBrlRate { get; set; }
    public bool AutoUpdateExchangeRate { get; set; }

    // Frete
    public decimal DefaultShippingCostUsd { get; set; }
    public bool UseEstimatedShipping { get; set; }

    // Filtros de Viabilidade
    public int MinSalesVolume { get; set; }
    public decimal MinSupplierRating { get; set; }
    public int MaxDeliveryDays { get; set; }

    // Pesos para Score
    public decimal WeightMargin { get; set; }
    public decimal WeightSales { get; set; }
    public decimal WeightRating { get; set; }
    public decimal WeightDelivery { get; set; }
}

public class UpdateMarketplaceConfigDto
{
    public decimal MinMarginPercent { get; set; }
    public decimal TargetMarginPercent { get; set; }
    public decimal MercadoLivreFixedFee { get; set; }
    public decimal MercadoLivrePercentFee { get; set; }
    public decimal MercadoLivreBoostFee { get; set; }
    public decimal ImportTaxPercent { get; set; }
    public decimal CompanyTaxPercent { get; set; }
    public decimal UsdToBrlRate { get; set; }
    public bool AutoUpdateExchangeRate { get; set; }
    public decimal DefaultShippingCostUsd { get; set; }
    public bool UseEstimatedShipping { get; set; }
    public int MinSalesVolume { get; set; }
    public decimal MinSupplierRating { get; set; }
    public int MaxDeliveryDays { get; set; }
    public decimal WeightMargin { get; set; }
    public decimal WeightSales { get; set; }
    public decimal WeightRating { get; set; }
    public decimal WeightDelivery { get; set; }
}
