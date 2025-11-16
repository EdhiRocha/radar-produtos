using RadarProdutos.Domain.Entities;
using RadarProdutos.Domain.DTOs;

namespace RadarProdutos.Application.Services
{
    // Simple score calculator that normalizes a few fields and combines them using weights.
    public static class ProductScoreCalculator
    {
        public static int CalculateScore(Product product, AnalysisConfig config, CompetitionInfoDto? competition, EngagementInfoDto? engagement)
        {
            // Normalize sales (orders) - assume higher is better
            var salesNorm = Normalize(product.Orders, 0, 1000);

            // Competition: Baixa -> 1, Media -> 0.5, Alta -> 0
            var compNorm = competition?.CompetitionLevel switch
            {
                "Baixa" => 1m,
                "Media" => 0.5m,
                "Alta" => 0m,
                _ => 0.5m
            };

            // Sentiment: Positivo -> 1, Misto -> 0.5, Negativo -> 0
            var sentNorm = engagement?.Sentiment switch
            {
                "Positivo" => 1m,
                "Misto" => 0.5m,
                "Negativo" => 0m,
                _ => 0.5m
            };

            // Margin normalized between configured min and max
            var marginNorm = NormalizeDecimal(product.MarginPercent, config.MinMarginPercent, config.MaxMarginPercent);

            var scoreDecimal = (config.WeightSales * (decimal)salesNorm)
                             + (config.WeightCompetition * compNorm)
                             + (config.WeightSentiment * sentNorm)
                             + (config.WeightMargin * marginNorm);

            // Convert to int 0-100
            var score = (int)System.Math.Round(System.Math.Min(100m, System.Math.Max(0m, scoreDecimal * 100m / (config.WeightSales + config.WeightCompetition + config.WeightSentiment + config.WeightMargin))));

            return score;
        }

        private static double Normalize(int value, int min, int max)
        {
            if (max <= min) return 0d;
            var v = (double)(value - min) / (max - min);
            return System.Math.Max(0d, System.Math.Min(1d, v));
        }

        private static decimal NormalizeDecimal(decimal value, decimal min, decimal max)
        {
            if (max <= min) return 0m;
            var v = (value - min) / (max - min);
            return System.Math.Max(0m, System.Math.Min(1m, v));
        }
    }
}
