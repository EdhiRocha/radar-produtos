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
    Task<AliHotProductsResponse?> GetHotProductsAsync(string keyword, string? categoryIds = null, decimal? minSalePrice = null, decimal? maxSalePrice = null, int pageNo = 1, int pageSize = 20, string sort = "SALE_PRICE_ASC", string platformProductType = "ALL");
    Task<AliCategoryResponse?> GetCategoriesAsync();
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

    public async Task<AliHotProductsResponse?> GetHotProductsAsync(
        string keyword,
        string? categoryIds = null,
        decimal? minSalePrice = null,
        decimal? maxSalePrice = null,
        int pageNo = 1,
        int pageSize = 20,
        string sort = "SALE_PRICE_ASC",
        string platformProductType = "ALL")
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
                { "method", "aliexpress.affiliate.hotproduct.query" },
                { "page_no", pageNo.ToString() },
                { "page_size", pageSize.ToString() },
                { "platform_product_type", platformProductType },
                { "sign_method", "md5" },
                { "sort", sort },
                { "target_currency", "USD" },
                { "target_language", "EN" },
                { "timestamp", timestamp },
                { "v", "2.0" }
            };

            if (!string.IsNullOrEmpty(_trackingId))
            {
                parameters.Add("tracking_id", _trackingId);
            }

            if (!string.IsNullOrEmpty(categoryIds))
            {
                parameters.Add("category_ids", categoryIds);
            }

            if (minSalePrice.HasValue)
            {
                parameters.Add("min_sale_price", (minSalePrice.Value * 100).ToString("F0")); // Converte para centavos
            }

            if (maxSalePrice.HasValue)
            {
                parameters.Add("max_sale_price", (maxSalePrice.Value * 100).ToString("F0")); // Converte para centavos
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

            Console.WriteLine($"🔥 Hot Products Request: {url.Substring(0, Math.Min(150, url.Length))}...");

            var response = await _httpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"🔥 Hot Products Response: {json.Substring(0, Math.Min(500, json.Length))}...");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Hot Products API Error: {response.StatusCode}");
                return null;
            }

            // Verifica se é uma resposta de erro
            if (json.Contains("error_response"))
            {
                Console.WriteLine($"❌ Hot Products API retornou erro: {json}");
                return null;
            }

            var apiResponse = JsonSerializer.Deserialize<AliHotProductsResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            var productsCount = apiResponse?.RespResult?.Result?.Products?.Count ?? 0;
            Console.WriteLine($"✅ Hot Products retornados: {productsCount}");
            Console.WriteLine($"📊 Total records: {apiResponse?.RespResult?.Result?.TotalRecordCount}, Page {apiResponse?.RespResult?.Result?.CurrentPageNo}/{apiResponse?.RespResult?.Result?.TotalPageNo}");

            return apiResponse;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Exception ao buscar Hot Products: {ex.Message}");
            return null;
        }
    }

    public async Task<AliCategoryResponse?> GetCategoriesAsync()
    {
        try
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

            // Parâmetros DEVEM estar em ordem alfabética para a assinatura
            var parameters = new Dictionary<string, string>
            {
                { "app_key", _appKey },
                { "format", "json" },
                { "method", "aliexpress.affiliate.category.get" },
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

            Console.WriteLine($"📂 Categories Request: {url.Substring(0, Math.Min(150, url.Length))}...");

            var response = await _httpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Categories API Error: {response.StatusCode}");
                return null;
            }

            var apiResponse = JsonSerializer.Deserialize<AliCategoryResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            var categoriesCount = apiResponse?.AliexpressAffiliateCategoryGetResponse?.RespResult?.Result?.Categories?.Category?.Count ?? 0;
            var totalCount = apiResponse?.AliexpressAffiliateCategoryGetResponse?.RespResult?.Result?.TotalResultCount ?? 0;
            Console.WriteLine($"✅ Categorias retornadas: {categoriesCount}/{totalCount}");

            return apiResponse;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Exception ao buscar Categorias: {ex.Message}");
            return null;
        }
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

// Classes para Hot Products API
public class AliHotProductsResponse
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("resp_result")]
    public AliHotProductsRespResult? RespResult { get; set; }

    [JsonPropertyName("request_id")]
    public string? RequestId { get; set; }
}

public class AliHotProductsRespResult
{
    [JsonPropertyName("resp_code")]
    public string? RespCode { get; set; }

    [JsonPropertyName("resp_msg")]
    public string? RespMsg { get; set; }

    [JsonPropertyName("result")]
    public AliHotProductsResult? Result { get; set; }
}

public class AliHotProductsResult
{
    [JsonPropertyName("current_page_no")]
    public string? CurrentPageNo { get; set; }

    [JsonPropertyName("current_record_count")]
    public string? CurrentRecordCount { get; set; }

    [JsonPropertyName("total_page_no")]
    public string? TotalPageNo { get; set; }

    [JsonPropertyName("total_record_count")]
    public string? TotalRecordCount { get; set; }

    [JsonPropertyName("products")]
    public List<AliHotProduct>? Products { get; set; }
}

public class AliHotProduct
{
    [JsonPropertyName("product_id")]
    public string? ProductId { get; set; }

    [JsonPropertyName("product_title")]
    public string? ProductTitle { get; set; }

    [JsonPropertyName("product_detail_url")]
    public string? ProductDetailUrl { get; set; }

    [JsonPropertyName("product_small_image_urls")]
    public List<string>? ProductSmallImageUrls { get; set; }

    [JsonPropertyName("product_main_image_url")]
    public string? ProductMainImageUrl { get; set; }

    [JsonPropertyName("target_sale_price")]
    public string? TargetSalePrice { get; set; }

    [JsonPropertyName("target_original_price")]
    public string? TargetOriginalPrice { get; set; }

    [JsonPropertyName("target_sale_price_currency")]
    public string? TargetSalePriceCurrency { get; set; }

    [JsonPropertyName("target_original_price_currency")]
    public string? TargetOriginalPriceCurrency { get; set; }

    [JsonPropertyName("original_price")]
    public string? OriginalPrice { get; set; }

    [JsonPropertyName("original_price_currency")]
    public string? OriginalPriceCurrency { get; set; }

    [JsonPropertyName("sale_price")]
    public string? SalePrice { get; set; }

    [JsonPropertyName("sale_price_currency")]
    public string? SalePriceCurrency { get; set; }

    [JsonPropertyName("discount")]
    public string? Discount { get; set; }

    [JsonPropertyName("lastest_volume")]
    public string? LastestVolume { get; set; }

    [JsonPropertyName("commission_rate")]
    public string? CommissionRate { get; set; }

    [JsonPropertyName("hot_product_commission_rate")]
    public string? HotProductCommissionRate { get; set; }

    [JsonPropertyName("evaluate_rate")]
    public string? EvaluateRate { get; set; }

    [JsonPropertyName("shop_url")]
    public string? ShopUrl { get; set; }

    [JsonPropertyName("shop_id")]
    public string? ShopId { get; set; }

    [JsonPropertyName("shop_name")]
    public string? ShopName { get; set; }

    [JsonPropertyName("first_level_category_id")]
    public string? FirstLevelCategoryId { get; set; }

    [JsonPropertyName("first_level_category_name")]
    public string? FirstLevelCategoryName { get; set; }

    [JsonPropertyName("second_level_category_id")]
    public string? SecondLevelCategoryId { get; set; }

    [JsonPropertyName("second_level_category_name")]
    public string? SecondLevelCategoryName { get; set; }

    [JsonPropertyName("platform_product_type")]
    public string? PlatformProductType { get; set; }
}

// Classes para Category API
public class AliCategoryResponse
{
    [JsonPropertyName("aliexpress_affiliate_category_get_response")]
    public AliCategoryGetResponse? AliexpressAffiliateCategoryGetResponse { get; set; }
}

public class AliCategoryGetResponse
{
    [JsonPropertyName("resp_result")]
    public AliCategoryRespResult? RespResult { get; set; }
}

public class AliCategoryRespResult
{
    [JsonPropertyName("resp_code")]
    public int RespCode { get; set; }

    [JsonPropertyName("resp_msg")]
    public string? RespMsg { get; set; }

    [JsonPropertyName("result")]
    public AliCategoryResult? Result { get; set; }
}

public class AliCategoryResult
{
    [JsonPropertyName("total_result_count")]
    public int TotalResultCount { get; set; }

    [JsonPropertyName("categories")]
    public AliCategoriesWrapper? Categories { get; set; }
}

public class AliCategoriesWrapper
{
    [JsonPropertyName("category")]
    public List<AliCategory>? Category { get; set; }
}

public class AliCategory
{
    [JsonPropertyName("category_id")]
    public long CategoryId { get; set; }

    [JsonPropertyName("category_name")]
    public string? CategoryName { get; set; }

    [JsonPropertyName("parent_category_id")]
    public long? ParentCategoryId { get; set; }
}

