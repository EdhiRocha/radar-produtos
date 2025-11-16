namespace RadarProdutos.Application.Requests;

public class HotProductsFilterDto
{
    public string Keyword { get; set; } = null!;
    public string? CategoryIds { get; set; } // ex: "111,222,333"
    public decimal? MinSalePrice { get; set; }
    public decimal? MaxSalePrice { get; set; }
    public int PageNo { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string Sort { get; set; } = "SALE_PRICE_ASC";
    public string PlatformProductType { get; set; } = "ALL";
}
