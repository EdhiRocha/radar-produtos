using RadarProdutos.Domain.Entities;
using RadarProdutos.Domain.DTOs;
using RadarProdutos.Application.DTOs;

namespace RadarProdutos.Application.Services
{
    // Simple score calculator that normalizes a few fields and combines them using weights.
    public static class ProductScoreCalculator
    {
        public static int CalculateScore(Product product, AnalysisConfig config, CompetitionInfoDto? competition, EngagementInfoDto? engagement)
        {
            return CalculateScoreInternal(
                product.Orders,
                product.Rating,
                product.MarginPercent,
                product.CompetitionLevel,
                product.Sentiment,
                config,
                competition,
                engagement,
                null,
                null,
                false
            );
        }

        // Sobrecarga para ProductDto com métricas adicionais
        public static int CalculateScore(ProductDto product, AnalysisConfig config)
        {
            return CalculateScoreInternal(
                product.Orders,
                product.Rating,
                product.MarginPercent,
                product.CompetitionLevel,
                product.Sentiment,
                config,
                null,
                null,
                product.ShippingDays,
                product.CommissionRate,
                product.HasVideo
            );
        }

        private static int CalculateScoreInternal(
            int orders,
            decimal rating,
            decimal marginPercent,
            string competitionLevel,
            string sentiment,
            AnalysisConfig config,
            CompetitionInfoDto? competition,
            EngagementInfoDto? engagement,
            int? shippingDays,
            decimal? commissionRate,
            bool hasVideo)
        {
            // Normalize sales (orders) com escala logarítmica para diferenciar melhor valores baixos
            var salesNorm = NormalizeSales(orders);

            // Competition: Baixa -> 1, Media -> 0.5, Alta -> 0
            var compNorm = (competition?.CompetitionLevel ?? competitionLevel) switch
            {
                "Baixa" => 1m,
                "Media" => 0.5m,
                "Alta" => 0m,
                _ => 0.5m
            };

            // Sentiment: Positivo -> 1, Misto -> 0.5, Negativo -> 0
            var sentNorm = (engagement?.Sentiment ?? sentiment) switch
            {
                "Positivo" => 1m,
                "Misto" => 0.5m,
                "Negativo" => 0m,
                _ => 0.5m
            };

            // Margin normalized between configured min and max
            var marginNorm = NormalizeDecimal(marginPercent, config.MinMarginPercent, config.MaxMarginPercent);

            // Rating também contribui para o score
            var ratingNorm = rating / 5m;

            var scoreDecimal = (config.WeightSales * (decimal)salesNorm)
                             + (config.WeightCompetition * compNorm)
                             + (config.WeightSentiment * sentNorm)
                             + (config.WeightMargin * marginNorm);

            // Bônus/Penalidades baseados nas métricas adicionais
            var bonusMultiplier = 1m;

            // Ajuste por rating: produtos com rating muito baixo têm penalidade, rating alto têm bônus
            var ratingBonus = (ratingNorm - 0.7m) * 0.2m; // -0.14 a +0.06
            bonusMultiplier += ratingBonus;

            // Bônus por ter vídeo (produtos com vídeo convertem melhor)
            if (hasVideo)
            {
                bonusMultiplier += 0.05m; // +5% no score
            }

            // Penalidade por prazo de entrega longo
            if (shippingDays.HasValue)
            {
                if (shippingDays.Value <= 15) bonusMultiplier += 0.08m;      // Entrega rápida +8%
                else if (shippingDays.Value <= 30) bonusMultiplier += 0.03m; // Entrega normal +3%
                else if (shippingDays.Value > 45) bonusMultiplier -= 0.10m;  // Entrega lenta -10%
            }

            // Bônus por alta comissão (produtos com comissão alta são mais promovidos pelo AliExpress)
            if (commissionRate.HasValue && commissionRate.Value > 5m)
            {
                bonusMultiplier += 0.05m; // +5% para comissão alta
            }

            scoreDecimal *= bonusMultiplier;

            // Convert to int 0-100
            var totalWeight = config.WeightSales + config.WeightCompetition + config.WeightSentiment + config.WeightMargin;
            var score = (int)System.Math.Round(System.Math.Min(100m, System.Math.Max(0m, scoreDecimal * 100m / totalWeight)));

            return score;
        }

        private static double NormalizeSales(int orders)
        {
            // Escala logarítmica para melhor diferenciação em valores baixos
            if (orders <= 0) return 0.05; // Produtos sem vendas ainda têm valor mínimo
            if (orders >= 10000) return 1.0;

            // log(orders + 1) / log(10001) para suavizar a curva
            return System.Math.Log(orders + 1) / System.Math.Log(10001);
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
