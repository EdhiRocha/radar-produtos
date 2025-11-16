using System.Threading.Tasks;
using RadarProdutos.Domain.Entities;

namespace RadarProdutos.Domain.Interfaces
{
    public interface IAnalysisConfigRepository
    {
        Task<AnalysisConfig?> GetAsync();
        Task SaveAsync(AnalysisConfig config);
    }
}
