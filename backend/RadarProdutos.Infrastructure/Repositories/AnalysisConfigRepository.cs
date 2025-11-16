using System.Linq;
using System.Threading.Tasks;
using RadarProdutos.Domain.Entities;
using RadarProdutos.Domain.Interfaces;
using RadarProdutos.Infrastructure.Data;

namespace RadarProdutos.Infrastructure.Repositories
{
    public class AnalysisConfigRepository : IAnalysisConfigRepository
    {
        private readonly AppDbContext _db;

        public AnalysisConfigRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<AnalysisConfig?> GetAsync()
        {
            var cfg = await Task.FromResult(_db.AnalysisConfigs.FirstOrDefault());
            return cfg;
        }

        public async Task SaveAsync(AnalysisConfig config)
        {
            var existing = _db.AnalysisConfigs.FirstOrDefault();
            if (existing == null)
            {
                _db.AnalysisConfigs.Add(config);
            }
            else
            {
                existing.MinMarginPercent = config.MinMarginPercent;
                existing.MaxMarginPercent = config.MaxMarginPercent;
                existing.WeightSales = config.WeightSales;
                existing.WeightCompetition = config.WeightCompetition;
                existing.WeightSentiment = config.WeightSentiment;
                existing.WeightMargin = config.WeightMargin;
            }

            await _db.SaveChangesAsync();
        }
    }
}
