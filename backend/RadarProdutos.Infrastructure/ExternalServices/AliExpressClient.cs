using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RadarProdutos.Domain.DTOs;

namespace RadarProdutos.Infrastructure.ExternalServices;

public interface IAliExpressClient
{
    Task<List<ScrapedProductDto>> SearchProductsAsync(string keyword);
    Task<List<ScrapedProductDto>> SearchProductsWithFiltersAsync(string? keyword, string? categoryIds, string? sort, decimal? maxSalePrice, decimal? minSalePrice, int pageNo, int pageSize);
    Task<AliHotProductsResponse?> GetHotProductsAsync(string keyword, string? categoryIds = null, decimal? minSalePrice = null, decimal? maxSalePrice = null, int pageNo = 1, int pageSize = 20, string sort = "SALE_PRICE_ASC", string platformProductType = "ALL");
    Task<AliCategoryResponse?> GetCategoriesAsync();
}

public class AliExpressClient : IAliExpressClient
{
    private readonly HttpClient _httpClient;
    private readonly string _appKey;
    private readonly string _appSecret;
    private readonly string? _trackingId;
    private readonly ILogger<AliExpressClient> _logger;

    public AliExpressClient(HttpClient httpClient, IConfiguration configuration, ILogger<AliExpressClient> logger)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        _appKey = configuration["AliExpress:AppKey"] ?? throw new InvalidOperationException("AliExpress:AppKey não configurado");
        _appSecret = configuration["AliExpress:AppSecret"] ?? throw new InvalidOperationException("AliExpress:AppSecret não configurado");
        _trackingId = configuration["AliExpress:TrackingId"];
        _logger = logger;
    }

    public async Task<List<ScrapedProductDto>> SearchProductsAsync(string keyword)
    {
        return await SearchProductsWithFiltersAsync(keyword, null, null, null, null, 1, 20);
    }

    public async Task<List<ScrapedProductDto>> SearchProductsWithFiltersAsync(
        string? keyword,
        string? categoryIds,
        string? sort,
        decimal? maxSalePrice,
        decimal? minSalePrice,
        int pageNo,
        int pageSize)
    {
        try
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

            // Parâmetros DEVEM estar em ordem alfabética para a assinatura
            var parameters = new Dictionary<string, string>
            {
                { "app_key", _appKey },
                { "format", "json" },
                { "method", "aliexpress.affiliate.product.query" },
                { "page_no", pageNo.ToString() },
                { "page_size", Math.Min(pageSize, 50).ToString() }, // Máximo 50
                { "sign_method", "md5" },
                { "timestamp", timestamp },
                { "v", "2.0" }
            };

            // Adiciona parâmetros opcionais
            if (!string.IsNullOrEmpty(keyword))
                parameters.Add("keywords", keyword);

            if (!string.IsNullOrEmpty(categoryIds))
                parameters.Add("category_ids", categoryIds);

            if (!string.IsNullOrEmpty(sort))
                parameters.Add("sort", sort);

            if (maxSalePrice.HasValue)
                parameters.Add("max_sale_price", ((int)(maxSalePrice.Value * 100)).ToString()); // Converte para centavos

            if (minSalePrice.HasValue)
                parameters.Add("min_sale_price", ((int)(minSalePrice.Value * 100)).ToString()); // Converte para centavos

            if (!string.IsNullOrEmpty(_trackingId))
                parameters.Add("tracking_id", _trackingId);

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

            _logger.LogDebug("Enviando requisição para AliExpress API: {Url}", url);

            var response = await _httpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            _logger.LogDebug("Resposta recebida da AliExpress API: {Response}", json.Length > 500 ? json.Substring(0, 500) + "..." : json);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Erro na API AliExpress: Status {StatusCode}, Erro: {ErrorContent}", response.StatusCode, errorContent);
                throw new HttpRequestException($"Erro na API AliExpress: {response.StatusCode} - {errorContent}");
            }

            var apiResponse = JsonSerializer.Deserialize<AliExpressResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            if (apiResponse?.AliexpressAffiliateProductQueryResponse?.RespResult?.Result?.Products?.Product == null)
            {
                _logger.LogWarning("Resposta da API AliExpress não contém produtos para keyword: {Keyword}", keyword);
                return new List<ScrapedProductDto>();
            }

            var products = apiResponse.AliexpressAffiliateProductQueryResponse.RespResult.Result.Products.Product
                .Select(p =>
                {
                    try
                    {
                        var rating = ParseDecimal(p.EvaluateRate ?? "0") / 20m;
                        var volumeFromApi = ParseInt(p.Volume ?? "0");

                        // Se a API não retorna volume (comum em APIs de afiliados), estima baseado em rating
                        var estimatedSales = volumeFromApi > 0 ? volumeFromApi : EstimateSalesFromRating(rating);

                        // Calcular desconto se houver preço original
                        var originalPrice = ParseDecimal(p.OriginalPrice ?? "0");
                        var salePrice = ParseDecimal(p.TargetSalePrice ?? p.SalePrice ?? "0");
                        var discount = p.Discount;
                        if (string.IsNullOrEmpty(discount) && originalPrice > 0 && salePrice > 0 && salePrice < originalPrice)
                        {
                            var discountPercent = ((originalPrice - salePrice) / originalPrice) * 100;
                            discount = $"{discountPercent:0}%";
                        }

                        return new ScrapedProductDto
                        {
                            ExternalId = p.ProductId?.ToString() ?? Guid.NewGuid().ToString(),
                            Name = p.ProductTitle ?? "Produto sem nome",
                            SupplierPrice = salePrice,
                            OriginalPrice = originalPrice > 0 ? originalPrice : null,
                            Discount = discount,
                            ImageUrl = p.ProductMainImageUrl,
                            SupplierUrl = p.ProductUrl,
                            ProductDetailUrl = p.ProductUrl,
                            ShopUrl = p.ShopUrl,
                            ShopName = p.ShopName,
                            PromotionLink = p.PromotionLink,
                            FirstLevelCategoryId = p.FirstLevelCategoryId?.ToString(),
                            FirstLevelCategoryName = p.FirstLevelCategoryName,
                            CommissionRate = ParseDecimal(p.CommissionRate ?? "0"),
                            ShippingDays = ExtractShippingDays(p.ShipToDays),
                            HasVideo = !string.IsNullOrEmpty(p.ProductVideoUrl),
                            AverageRating = rating,
                            TotalSales = estimatedSales,
                            Rating = rating,
                            Orders = estimatedSales,
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

            _logger.LogInformation(
                "Produtos AliExpress - Total: {TotalProducts}, Após filtro: {FilteredProducts}, Keyword: {Keyword}",
                products.Count,
                filteredProducts.Count,
                keyword);

            return filteredProducts;
        }
        catch (HttpRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exceção ao buscar produtos para keyword: {Keyword}", keyword);
            throw new HttpRequestException($"Erro inesperado ao buscar produtos: {ex.Message}", ex);
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
        _logger.LogDebug("Signature base: {SignatureBase}", signString.Substring(0, Math.Min(100, signString.Length)) + "...");

        using var md5 = MD5.Create();
        var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(signString));
        var signature = BitConverter.ToString(hashBytes).Replace("-", "").ToUpper();

        _logger.LogDebug("Signature gerada: {Signature}", signature);
        return signature;
    }

    private static int EstimateSalesFromRating(decimal rating)
    {
        // Estimativa conservadora: produtos encontrados na busca com bom rating 
        // provavelmente têm algum volume de vendas
        return rating switch
        {
            >= 4.7m => 800,   // Excelente rating = alta demanda
            >= 4.5m => 500,   // Muito bom
            >= 4.2m => 300,   // Bom
            >= 4.0m => 150,   // Razoável
            >= 3.5m => 50,    // Aceitável
            _ => 0            // Baixo rating = sem estimativa
        };
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

        _logger.LogDebug("Palavras-chave para filtro: {Keywords}", string.Join(", ", keywordParts));

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
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            foreach (var sp in scoredProducts.OrderByDescending(s => s.Score).Take(3))
            {
                _logger.LogDebug(
                    "Produto filtrado: {ProductName}... (Score: {Score:P0}, Matches: {MatchCount}/{TotalKeywords})",
                    sp.Product.Name?.Substring(0, Math.Min(50, sp.Product.Name.Length)),
                    sp.Score,
                    sp.MatchCount,
                    keywordParts.Count);
            }
        }

        // Se nenhum produto passou no filtro, retorna os 5 melhores por score
        if (!relevantProducts.Any())
        {
            _logger.LogWarning("Nenhum produto passou no filtro de relevância mínima. Retornando top 5 por score");
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

            _logger.LogDebug("Hot Products Request: {Url}", url.Substring(0, Math.Min(150, url.Length)) + "...");

            var response = await _httpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            _logger.LogDebug("Hot Products Response: {Response}", json.Substring(0, Math.Min(500, json.Length)) + "...");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Hot Products API Error: Status {StatusCode}", response.StatusCode);
                return null;
            }

            // Verifica se é uma resposta de erro
            if (json.Contains("error_response"))
            {
                _logger.LogError("Hot Products API retornou erro: {Response}", json);
                return null;
            }

            var apiResponse = JsonSerializer.Deserialize<AliHotProductsResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            var productsCount = apiResponse?.RespResult?.Result?.Products?.Count ?? 0;
            _logger.LogInformation(
                "Hot Products retornados: {ProductsCount}, Total records: {TotalRecords}, Page {CurrentPage}/{TotalPages}",
                productsCount,
                apiResponse?.RespResult?.Result?.TotalRecordCount,
                apiResponse?.RespResult?.Result?.CurrentPageNo,
                apiResponse?.RespResult?.Result?.TotalPageNo);

            return apiResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exceção ao buscar Hot Products para keyword: {Keyword}", keyword);
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

            _logger.LogDebug("Categories Request: {Url}", url.Substring(0, Math.Min(150, url.Length)) + "...");

            var response = await _httpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Categories API Error: Status {StatusCode}", response.StatusCode);
                return null;
            }

            var apiResponse = JsonSerializer.Deserialize<AliCategoryResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            var categoriesCount = apiResponse?.AliexpressAffiliateCategoryGetResponse?.RespResult?.Result?.Categories?.Category?.Count ?? 0;
            var totalCount = apiResponse?.AliexpressAffiliateCategoryGetResponse?.RespResult?.Result?.TotalResultCount ?? 0;
            _logger.LogInformation("Categorias retornadas: {CategoriesCount}/{TotalCount}", categoriesCount, totalCount);

            return apiResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exceção ao buscar Categorias");
            return null;
        }
    }

    // Método auxiliar para extrair dias de envio
    private static int? ExtractShippingDays(string? shipToDays)
    {
        if (string.IsNullOrWhiteSpace(shipToDays)) return null;

        // Extrai números do texto "ship to RU in 7 days"
        var numbers = new string(shipToDays.Where(char.IsDigit).ToArray());
        if (string.IsNullOrEmpty(numbers)) return null;

        return int.TryParse(numbers, out var days) ? days : null;
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

    [JsonPropertyName("discount")]
    public string? Discount { get; set; }

    [JsonPropertyName("shop_url")]
    public string? ShopUrl { get; set; }

    [JsonPropertyName("shop_name")]
    public string? ShopName { get; set; }

    [JsonPropertyName("promotion_link")]
    public string? PromotionLink { get; set; }

    [JsonPropertyName("first_level_category_id")]
    public long? FirstLevelCategoryId { get; set; }

    [JsonPropertyName("first_level_category_name")]
    public string? FirstLevelCategoryName { get; set; }

    [JsonPropertyName("commission_rate")]
    public string? CommissionRate { get; set; }

    [JsonPropertyName("ship_to_days")]
    public string? ShipToDays { get; set; }

    [JsonPropertyName("product_video_url")]
    public string? ProductVideoUrl { get; set; }
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
    public long? FirstLevelCategoryId { get; set; }

    [JsonPropertyName("first_level_category_name")]
    public string? FirstLevelCategoryName { get; set; }

    [JsonPropertyName("second_level_category_id")]
    public string? SecondLevelCategoryId { get; set; }

    [JsonPropertyName("second_level_category_name")]
    public string? SecondLevelCategoryName { get; set; }

    [JsonPropertyName("platform_product_type")]
    public string? PlatformProductType { get; set; }

    [JsonPropertyName("ship_to_days")]
    public string? ShipToDays { get; set; }

    [JsonPropertyName("promotion_link")]
    public string? PromotionLink { get; set; }

    [JsonPropertyName("product_video_url")]
    public string? ProductVideoUrl { get; set; }

    [JsonPropertyName("relevant_market_commission_rate")]
    public string? RelevantMarketCommissionRate { get; set; }

    [JsonPropertyName("app_sale_price")]
    public string? AppSalePrice { get; set; }

    [JsonPropertyName("app_sale_price_currency")]
    public string? AppSalePriceCurrency { get; set; }
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

