using Microsoft.AspNetCore.Mvc;
using RadarProdutos.Application.DTOs;
using RadarProdutos.Domain.Entities;
using RadarProdutos.Domain.Interfaces;
using System.Threading.Tasks;

namespace RadarProdutos.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfigController : ControllerBase
    {
        private readonly IAnalysisConfigRepository _configRepo;

        public ConfigController(IAnalysisConfigRepository configRepo)
        {
            _configRepo = configRepo;
        }

        // GET /api/config
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var cfg = await _configRepo.GetAsync();
            if (cfg == null)
                return NotFound("Config not found.");

            return Ok(new AnalysisConfigDto
            {
                Id = cfg.Id,
                MinMarginPercent = cfg.MinMarginPercent,
                MaxMarginPercent = cfg.MaxMarginPercent,
                WeightSales = cfg.WeightSales,
                WeightCompetition = cfg.WeightCompetition,
                WeightSentiment = cfg.WeightSentiment,
                WeightMargin = cfg.WeightMargin
            });
        }

        // PUT /api/config
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] AnalysisConfigDto dto)
        {
            var cfg = new AnalysisConfig
            {
                Id = dto.Id,
                MinMarginPercent = dto.MinMarginPercent,
                MaxMarginPercent = dto.MaxMarginPercent,
                WeightSales = dto.WeightSales,
                WeightCompetition = dto.WeightCompetition,
                WeightSentiment = dto.WeightSentiment,
                WeightMargin = dto.WeightMargin
            };

            await _configRepo.SaveAsync(cfg);
            return Ok(cfg);
        }
    }
}
