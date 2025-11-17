using Microsoft.AspNetCore.Mvc;
using RadarProdutos.Application.DTOs;
using RadarProdutos.Domain.Entities;
using RadarProdutos.Domain.Interfaces;

namespace RadarProdutos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MarketplaceConfigController : ControllerBase
{
    private readonly IMarketplaceConfigRepository _configRepo;

    public MarketplaceConfigController(IMarketplaceConfigRepository configRepo)
    {
        _configRepo = configRepo;
    }

    // GET /api/marketplaceconfig
    [HttpGet]
    public async Task<IActionResult> GetConfig()
    {
        var config = await _configRepo.GetAsync();

        if (config == null)
            return NotFound(new { message = "Configuração não encontrada" });

        var dto = new MarketplaceConfigDto
        {
            Id = config.Id,
            MinMarginPercent = config.MinMarginPercent,
            TargetMarginPercent = config.TargetMarginPercent,
            MercadoLivreFixedFee = config.MercadoLivreFixedFee,
            MercadoLivrePercentFee = config.MercadoLivrePercentFee,
            MercadoLivreBoostFee = config.MercadoLivreBoostFee,
            ImportTaxPercent = config.ImportTaxPercent,
            CompanyTaxPercent = config.CompanyTaxPercent,
            UsdToBrlRate = config.UsdToBrlRate,
            AutoUpdateExchangeRate = config.AutoUpdateExchangeRate,
            DefaultShippingCostUsd = config.DefaultShippingCostUsd,
            UseEstimatedShipping = config.UseEstimatedShipping,
            MinSalesVolume = config.MinSalesVolume,
            MinSupplierRating = config.MinSupplierRating,
            MaxDeliveryDays = config.MaxDeliveryDays,
            WeightMargin = config.WeightMargin,
            WeightSales = config.WeightSales,
            WeightRating = config.WeightRating,
            WeightDelivery = config.WeightDelivery
        };

        return Ok(dto);
    }

    // PUT /api/marketplaceconfig
    [HttpPut]
    public async Task<IActionResult> UpdateConfig([FromBody] UpdateMarketplaceConfigDto dto)
    {
        var config = await _configRepo.GetAsync();

        if (config == null)
        {
            // Criar nova configuração
            config = new MarketplaceConfig
            {
                CreatedAt = DateTime.UtcNow
            };
        }

        // Atualizar valores
        config.MinMarginPercent = dto.MinMarginPercent;
        config.TargetMarginPercent = dto.TargetMarginPercent;
        config.MercadoLivreFixedFee = dto.MercadoLivreFixedFee;
        config.MercadoLivrePercentFee = dto.MercadoLivrePercentFee;
        config.MercadoLivreBoostFee = dto.MercadoLivreBoostFee;
        config.ImportTaxPercent = dto.ImportTaxPercent;
        config.CompanyTaxPercent = dto.CompanyTaxPercent;
        config.UsdToBrlRate = dto.UsdToBrlRate;
        config.AutoUpdateExchangeRate = dto.AutoUpdateExchangeRate;
        config.DefaultShippingCostUsd = dto.DefaultShippingCostUsd;
        config.UseEstimatedShipping = dto.UseEstimatedShipping;
        config.MinSalesVolume = dto.MinSalesVolume;
        config.MinSupplierRating = dto.MinSupplierRating;
        config.MaxDeliveryDays = dto.MaxDeliveryDays;
        config.WeightMargin = dto.WeightMargin;
        config.WeightSales = dto.WeightSales;
        config.WeightRating = dto.WeightRating;
        config.WeightDelivery = dto.WeightDelivery;

        var updated = await _configRepo.UpdateAsync(config);

        return Ok(new { message = "Configuração atualizada com sucesso", config = updated });
    }
}
