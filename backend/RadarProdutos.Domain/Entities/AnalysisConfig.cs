using System;

namespace RadarProdutos.Domain.Entities
{
    // Configurações que impactam o cálculo de score
    public class AnalysisConfig
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
