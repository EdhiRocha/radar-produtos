using RadarProdutos.Application.DTOs;
using RadarProdutos.Infrastructure.ExternalServices;

namespace RadarProdutos.Application.Mappers;

public static class HotProductMapper
{
    public static ProductDto ToProductDto(AliHotProduct src)
    {
        var supplierPrice = ParseDecimal(src.TargetSalePrice ?? "0");
        var originalPrice = ParseDecimal(src.TargetOriginalPrice ?? "0");
        var estimatedSalePrice = supplierPrice * 2.5m; // Heurística: 2.5x o preço do fornecedor
        var marginPercent = estimatedSalePrice > 0
            ? (estimatedSalePrice - supplierPrice) / estimatedSalePrice * 100
            : 0;

        var rating = ParseDecimal(src.EvaluateRate ?? "0") / 20m; // Converte de 0-100 para 0-5
        var sales = ParseInt(src.LastestVolume ?? "0");

        // Pega a primeira imagem disponível (prioriza main, depois small)
        var imageUrl = src.ProductMainImageUrl ?? src.ProductSmallImageUrls?.FirstOrDefault() ?? "";

        return new ProductDto
        {
            Id = Guid.NewGuid(),
            ExternalId = src.ProductId ?? "0",
            Name = src.ProductTitle ?? "Produto sem nome",
            Supplier = "AliExpress",
            ImageUrl = imageUrl,
            SupplierPrice = supplierPrice,
            EstimatedSalePrice = estimatedSalePrice,
            MarginPercent = decimal.Round(marginPercent, 2),
            Rating = rating,
            Orders = sales,
            CompetitionLevel = DetermineCompetitionLevel(sales),
            Sentiment = DetermineSentiment(rating),
            Score = 0 // Será calculado pelo ProductScoreCalculator
        };
    }

    private static int ParseInt(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return 0;

        var cleaned = new string(value.Where(char.IsDigit).ToArray());
        if (string.IsNullOrEmpty(cleaned)) return 0;

        return int.TryParse(cleaned, out var result) ? result : 0;
    }

    private static string DetermineCompetitionLevel(int sales)
    {
        return sales switch
        {
            > 5000 => "Alta",
            > 1000 => "Media",
            _ => "Baixa"
        };
    }

    private static string DetermineSentiment(decimal rating)
    {
        return rating switch
        {
            >= 4.5m => "Positivo",
            >= 3.5m => "Misto",
            _ => "Negativo"
        };
    }

    private static decimal ParseDecimal(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return 0;

        var cleaned = new string(value.Where(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());
        if (string.IsNullOrEmpty(cleaned)) return 0;

        cleaned = cleaned.Replace(',', '.');

        return decimal.TryParse(cleaned, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var result) ? result : 0;
    }
}
