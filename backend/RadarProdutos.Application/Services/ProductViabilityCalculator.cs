using RadarProdutos.Domain.Entities;

namespace RadarProdutos.Application.Services;

public class ProductViabilityCalculator
{
    public static ProductViabilityResult Calculate(
        decimal productPriceUsd,
        decimal shippingCostUsd,
        int salesVolume,
        decimal supplierRating,
        MarketplaceConfig config)
    {
        var result = new ProductViabilityResult();

        // 1. Converter USD para BRL
        var productPriceBrl = productPriceUsd * config.UsdToBrlRate;
        var shippingCostBrl = shippingCostUsd * config.UsdToBrlRate;

        // 2. Calcular impostos de importação (60% sobre produto + frete)
        var importBase = productPriceBrl + shippingCostBrl;
        var importTax = importBase * (config.ImportTaxPercent / 100);

        // 3. Custo total de aquisição
        var totalAcquisitionCost = productPriceBrl + shippingCostBrl + importTax;

        // 4. Calcular preço de venda para atingir margem alvo
        // Fórmula: PreçoVenda = (CustoTotal + TaxasFixas) / (1 - TaxasVariáveis% - MargemDesejada%)
        var variableFees = (config.MercadoLivrePercentFee + config.MercadoLivreBoostFee + config.CompanyTaxPercent) / 100;
        var targetMarginDecimal = config.TargetMarginPercent / 100;

        var suggestedPrice = (totalAcquisitionCost + config.MercadoLivreFixedFee) / (1 - variableFees - targetMarginDecimal);

        // 5. Calcular taxas do Mercado Livre
        var mlVariableFee = suggestedPrice * variableFees;
        var totalMlFees = config.MercadoLivreFixedFee + mlVariableFee;

        // 6. Calcular margem real
        var totalCosts = totalAcquisitionCost + totalMlFees;
        var netProfit = suggestedPrice - totalCosts;
        var realMarginPercent = (netProfit / suggestedPrice) * 100;

        // 7. Calcular ROI
        var roi = (netProfit / totalAcquisitionCost) * 100;

        // 8. Verificar viabilidade
        var isViable =
            realMarginPercent >= config.MinMarginPercent &&
            salesVolume >= config.MinSalesVolume &&
            supplierRating >= config.MinSupplierRating;

        // 9. Calcular score de viabilidade (0-100)
        var marginScore = Math.Min((realMarginPercent / config.TargetMarginPercent) * 100, 100);
        var salesScore = Math.Min((salesVolume / (config.MinSalesVolume * 10m)) * 100, 100);
        var ratingScore = (supplierRating / 5m) * 100;

        var totalWeight = config.WeightMargin + config.WeightSales + config.WeightRating;
        var viabilityScore = (
            (marginScore * config.WeightMargin) +
            (salesScore * config.WeightSales) +
            (ratingScore * config.WeightRating)
        ) / totalWeight;

        // Preencher resultado
        result.ProductPriceUsd = productPriceUsd;
        result.ProductPriceBrl = productPriceBrl;
        result.ShippingCostUsd = shippingCostUsd;
        result.ShippingCostBrl = shippingCostBrl;
        result.ImportTax = importTax;
        result.TotalAcquisitionCost = totalAcquisitionCost;
        result.MercadoLivreFixedFee = config.MercadoLivreFixedFee;
        result.MercadoLivreVariableFee = mlVariableFee;
        result.TotalMercadoLivreFees = totalMlFees;
        result.SuggestedSalePrice = Math.Round(suggestedPrice, 2);
        result.TotalCosts = totalCosts;
        result.NetProfit = netProfit;
        result.RealMarginPercent = Math.Round(realMarginPercent, 2);
        result.ROI = Math.Round(roi, 2);
        result.IsViable = isViable;
        result.ViabilityScore = Math.Round(viabilityScore, 2);
        result.ExchangeRate = config.UsdToBrlRate;

        return result;
    }
}

public class ProductViabilityResult
{
    // Custos Base
    public decimal ProductPriceUsd { get; set; }
    public decimal ProductPriceBrl { get; set; }
    public decimal ShippingCostUsd { get; set; }
    public decimal ShippingCostBrl { get; set; }
    public decimal ImportTax { get; set; }
    public decimal TotalAcquisitionCost { get; set; }

    // Taxas Mercado Livre
    public decimal MercadoLivreFixedFee { get; set; }
    public decimal MercadoLivreVariableFee { get; set; }
    public decimal TotalMercadoLivreFees { get; set; }

    // Análise de Rentabilidade
    public decimal SuggestedSalePrice { get; set; }
    public decimal TotalCosts { get; set; }
    public decimal NetProfit { get; set; }
    public decimal RealMarginPercent { get; set; }
    public decimal ROI { get; set; }

    // Viabilidade
    public bool IsViable { get; set; }
    public decimal ViabilityScore { get; set; }

    // Metadados
    public decimal ExchangeRate { get; set; }
}
