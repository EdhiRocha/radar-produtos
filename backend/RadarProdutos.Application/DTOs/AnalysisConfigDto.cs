namespace RadarProdutos.Application.DTOs
{
    public class AnalysisConfigDto
    {
        public int Id { get; set; }
        public decimal MinMarginPercent { get; set; }
        public decimal MaxMarginPercent { get; set; }
        public decimal WeightSales { get; set; }
        public decimal WeightCompetition { get; set; }
        public decimal WeightSentiment { get; set; }
        public decimal WeightMargin { get; set; }
    }
}
