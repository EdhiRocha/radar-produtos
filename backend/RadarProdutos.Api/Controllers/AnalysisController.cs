using Microsoft.AspNetCore.Mvc;
using RadarProdutos.Application.Requests;
using RadarProdutos.Application.Services;
using System.Threading.Tasks;

namespace RadarProdutos.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalysisController : ControllerBase
    {
        private readonly IAnalysisService _analysisService;

        public AnalysisController(IAnalysisService analysisService)
        {
            _analysisService = analysisService;
        }

        // POST /api/analysis/run
        [HttpPost("run")]
        public async Task<IActionResult> Run([FromBody] RunAnalysisRequest request)
        {
            // Se não informar keyword, busca produtos "quentes" (trending)
            if (string.IsNullOrWhiteSpace(request.Keyword))
            {
                request.Keyword = "smart watch"; // Categoria popular padrão
            }

            var results = await _analysisService.RunAnalysisAsync(request);
            return Ok(results);
        }

        // GET /api/analysis/latest
        [HttpGet("latest")]
        public async Task<IActionResult> GetLatest()
        {
            var analysis = await _analysisService.GetLatestAnalysisAsync();
            if (analysis == null) return NotFound("No analysis found.");

            return Ok(analysis);
        }
    }
}
