using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using RadarProdutos.Domain.DTOs;

namespace RadarProdutos.Infrastructure.ExternalServices;

public interface IAliExpressClient
{
    Task<List<ScrapedProductDto>> SearchProductsAsync(string keyword);
}

public class AliExpressClient : IAliExpressClient
{
    private readonly HttpClient _httpClient;
    private readonly string _appKey;
    private readonly string _appSecret;
    private readonly string? _trackingId;

    public AliExpressClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        _appKey = configuration["AliExpress:AppKey"] ?? throw new InvalidOperationException("AliExpress:AppKey não configurado");
        _appSecret = configuration["AliExpress:AppSecret"] ?? throw new InvalidOperationException("AliExpress:AppSecret não configurado");
        _trackingId = configuration["AliExpress:TrackingId"];
    }

    public async Task<List<ScrapedProductDto>> SearchProductsAsync(string keyword)
    {
        try
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

            // Parâmetros DEVEM estar em ordem alfabética para a assinatura
            var parameters = new Dictionary<string, string>
            {
                { "app_key", _appKey },
                { "format", "json" },
                { "keywords", keyword },
                { "method", "aliexpress.affiliate.product.query" },
                { "page_size", "20" },
                { "sign_method", "md5" },
                { "timestamp", timestamp },
                { "v", "2.0" }
            };

            if (!string.IsNullOrEmpty(_trackingId))
            {
                parameters.Add("tracking_id", _trackingId);
            }

            // Ordena alfabeticamente e gera assinatura
            var sortedParams = parameters.OrderBy(p => p.Key).ToList();
            var sign = GenerateSignature(sortedParams);

            // Constrói URL
            var queryParams = new List<string>();
            foreach (var param in sortedParams)
            {
                queryParams.Add($"{param.Key}={Uri.EscapeDataString(param.Value)}");
            }
            queryParams.Add($"sign={sign}");

            var queryString = string.Join("&", queryParams);
            var url = $"https://api-sg.aliexpress.com/sync?{queryString}";

            Console.WriteLine($"Requesting: {url}");

            var response = await _httpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Response: {json}");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"API Error: {response.StatusCode}. Using mock data.");
                return GetMockProducts(keyword);
            }

            var apiResponse = JsonSerializer.Deserialize<AliExpressResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            if (apiResponse?.AliexpressAffiliateProductQueryResponse?.RespResult?.Result?.Products?.Product == null)
            {
                return GetMockProducts(keyword);
            }

            var products = apiResponse.AliexpressAffiliateProductQueryResponse.RespResult.Result.Products.Product
                .Select(p =>
                {
                    try
                    {
                        return new ScrapedProductDto
                        {
                            ExternalId = p.ProductId?.ToString() ?? Guid.NewGuid().ToString(),
                            Name = p.ProductTitle ?? "Produto sem nome",
                            SupplierPrice = ParseDecimal(p.TargetSalePrice ?? p.SalePrice ?? p.OriginalPrice ?? "0"),
                            ImageUrl = p.ProductMainImageUrl,
                            SupplierUrl = p.ProductUrl,
                            AverageRating = ParseDecimal(p.EvaluateRate ?? "0") / 20m,
                            TotalSales = ParseInt(p.Volume ?? "0"),
                            Rating = ParseDecimal(p.EvaluateRate ?? "0") / 20m,
                            Orders = ParseInt(p.Volume ?? "0"),
                            Supplier = "AliExpress"
                        };
                    }
                    catch
                    {
                        return null;
                    }
                })
                .Where(p => p != null)
                .Cast<ScrapedProductDto>()
                .ToList();

            // Filtra produtos relevantes baseado nas palavras-chave principais
            var filteredProducts = FilterRelevantProducts(products, keyword);

            Console.WriteLine($"Produtos retornados pela API: {products.Count}");
            Console.WriteLine($"Produtos após filtro de relevância: {filteredProducts.Count}");

            return filteredProducts;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}. Using mock data.");
            return GetMockProducts(keyword);
        }
    }

    private string GenerateSignature(List<KeyValuePair<string, string>> sortedParameters)
    {
        // Formato AliExpress: SECRET + key1value1key2value2... + SECRET
        var sb = new StringBuilder();
        sb.Append(_appSecret);

        foreach (var param in sortedParameters)
        {
            sb.Append(param.Key);
            sb.Append(param.Value);
        }

        sb.Append(_appSecret);

        var signString = sb.ToString();
        Console.WriteLine($"Sign base: {signString.Substring(0, Math.Min(100, signString.Length))}...");

        using var md5 = MD5.Create();
        var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(signString));
        var signature = BitConverter.ToString(hashBytes).Replace("-", "").ToUpper();

        Console.WriteLine($"Signature: {signature}");
        return signature;
    }

    private static decimal ParseDecimal(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return 0;

        // Remove caracteres não numéricos exceto ponto e vírgula
        var cleaned = new string(value.Where(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());

        if (string.IsNullOrEmpty(cleaned)) return 0;

        // Substitui vírgula por ponto
        cleaned = cleaned.Replace(',', '.');

        return decimal.TryParse(cleaned, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var result) ? result : 0;
    }

    private static int ParseInt(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return 0;

        var cleaned = new string(value.Where(char.IsDigit).ToArray());

        return int.TryParse(cleaned, out var result) ? result : 0;
    }

    private List<ScrapedProductDto> FilterRelevantProducts(List<ScrapedProductDto> products, string keyword)
    {
        // Extrai palavras-chave principais (ignora palavras muito comuns)
        var stopWords = new[] { "the", "a", "an", "and", "or", "for", "with", "de", "da", "do", "para", "com", "e", "ou" };

        var keywordParts = keyword.ToLowerInvariant()
            .Split(new[] { ' ', ',', '.', '-', '_' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 2 && !stopWords.Contains(w))
            .Take(5) // Primeiras 5 palavras relevantes
            .ToList();

        if (!keywordParts.Any())
            return products;

        Console.WriteLine($"Palavras-chave para filtro: {string.Join(", ", keywordParts)}");

        // Calcula score de relevância para cada produto
        var scoredProducts = products.Select(p =>
        {
            var productNameLower = p.Name?.ToLowerInvariant() ?? "";
            var matchCount = keywordParts.Count(kw => productNameLower.Contains(kw));
            var relevanceScore = (double)matchCount / keywordParts.Count;

            return new { Product = p, Score = relevanceScore, MatchCount = matchCount };
        }).ToList();

        // Retorna apenas produtos com pelo menos 40% de relevância (2 de 5 palavras)
        var minRelevance = 0.4;
        var relevantProducts = scoredProducts
            .Where(sp => sp.Score >= minRelevance)
            .OrderByDescending(sp => sp.Score)
            .ThenByDescending(sp => sp.Product.TotalSales)
            .Select(sp => sp.Product)
            .ToList();

        // Log de produtos filtrados
        foreach (var sp in scoredProducts.OrderByDescending(s => s.Score).Take(3))
        {
            Console.WriteLine($"  {sp.Product.Name?.Substring(0, Math.Min(50, sp.Product.Name.Length))}... " +
                            $"(Score: {sp.Score:P0}, Matches: {sp.MatchCount}/{keywordParts.Count})");
        }

        // Se nenhum produto passou no filtro, retorna os 5 melhores por score
        if (!relevantProducts.Any())
        {
            Console.WriteLine("⚠️ Nenhum produto passou no filtro de relevância mínima. Retornando top 5 por score.");
            return scoredProducts
                .OrderByDescending(sp => sp.Score)
                .ThenByDescending(sp => sp.Product.TotalSales)
                .Take(5)
                .Select(sp => sp.Product)
                .ToList();
        }

        return relevantProducts;
    }

    private List<ScrapedProductDto> GetMockProducts(string keyword)
    {
        return new List<ScrapedProductDto>
        {
            new()
            {
                ExternalId = "mock-001",
                Name = $"Produto Mock 1 - {keyword}",
                Supplier = "AliExpress",
                SupplierPrice = 15.99m,
                ImageUrl = "https://via.placeholder.com/300x300?text=Produto+1",
                SupplierUrl = "https://aliexpress.com/item/mock-001",
                AverageRating = 4.5m,
                Rating = 4.5m,
                TotalSales = 1523,
                Orders = 1523
            },
            new()
            {
                ExternalId = "mock-002",
                Name = $"Produto Mock 2 - {keyword}",
                Supplier = "AliExpress",
                SupplierPrice = 22.50m,
                ImageUrl = "https://via.placeholder.com/300x300?text=Produto+2",
                SupplierUrl = "https://aliexpress.com/item/mock-002",
                AverageRating = 4.8m,
                Rating = 4.8m,
                TotalSales = 3847,
                Orders = 3847
            }
        };
    }
}

// Classes para deserialização da resposta da API AliExpress
public class AliExpressResponse
{
    [JsonPropertyName("aliexpress_affiliate_product_query_response")]
    public AliexpressAffiliateProductQueryResponse? AliexpressAffiliateProductQueryResponse { get; set; }
}

public class AliexpressAffiliateProductQueryResponse
{
    [JsonPropertyName("resp_result")]
    public RespResult? RespResult { get; set; }
}

public class RespResult
{
    [JsonPropertyName("result")]
    public Result? Result { get; set; }
}

public class Result
{
    [JsonPropertyName("products")]
    public Products? Products { get; set; }

    [JsonPropertyName("total_record_count")]
    public int TotalRecordCount { get; set; }
}

public class Products
{
    [JsonPropertyName("product")]
    public List<Product>? Product { get; set; }
}

public class Product
{
    [JsonPropertyName("product_id")]
    public long? ProductId { get; set; }

    [JsonPropertyName("product_title")]
    public string? ProductTitle { get; set; }

    [JsonPropertyName("product_main_image_url")]
    public string? ProductMainImageUrl { get; set; }

    [JsonPropertyName("product_detail_url")]
    public string? ProductUrl { get; set; }

    [JsonPropertyName("sale_price")]
    public string? SalePrice { get; set; }

    [JsonPropertyName("target_sale_price")]
    public string? TargetSalePrice { get; set; }

    [JsonPropertyName("evaluate_rate")]
    public string? EvaluateRate { get; set; }

    [JsonPropertyName("volume")]
    public string? Volume { get; set; }

    [JsonPropertyName("original_price")]
    public string? OriginalPrice { get; set; }
}
