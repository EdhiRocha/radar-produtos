namespace RadarProdutos.Application.Requests
{
    public class RunAnalysisRequest
    {
        /// <summary>
        /// Filter products by keywords. eg: mp3
        /// </summary>
        public string? Keyword { get; set; }

        /// <summary>
        /// List of category ID, you can get category ID via "get category" API https://developers.aliexpress.com/en/doc.htm?docId=45801&docType=2
        /// </summary>
        public string? CategoryIds { get; set; }

        /// <summary>
        /// Sort by: SALE_PRICE_ASC, SALE_PRICE_DESC, LAST_VOLUME_ASC, LAST_VOLUME_DESC
        /// </summary>
        public string? Sort { get; set; }

        /// <summary>
        /// Filter products by highest price, unit cent
        /// </summary>
        public decimal? MaxSalePrice { get; set; }

        /// <summary>
        /// Filter products by lowest price, unit cent
        /// </summary>
        public decimal? MinSalePrice { get; set; }

        /// <summary>
        /// Page number (default: 1)
        /// </summary>
        public int PageNo { get; set; } = 1;

        /// <summary>
        /// Record count of each page (1-50, default: 20)
        /// </summary>
        public int PageSize { get; set; } = 20;
    }
}
