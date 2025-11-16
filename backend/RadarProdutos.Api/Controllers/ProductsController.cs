using Microsoft.AspNetCore.Mvc;
using RadarProdutos.Application.DTOs;
using RadarProdutos.Application.Requests;
using RadarProdutos.Application.Services;
using RadarProdutos.Domain.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RadarProdutos.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepo;
        private readonly IHotProductsService _hotProductsService;

        public ProductsController(IProductRepository productRepo, IHotProductsService hotProductsService)
        {
            _productRepo = productRepo;
            _hotProductsService = hotProductsService;
        }

        // GET /api/products?page=1&pageSize=20&minMargin=15&competitionLevel=Baixa&sentiment=Positivo
        [HttpGet]
        public async Task<IActionResult> GetProducts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] decimal? minMargin = null,
            [FromQuery] string? competitionLevel = null,
            [FromQuery] string? sentiment = null)
        {
            var products = await _productRepo.GetPagedAsync(page, pageSize, minMargin, competitionLevel, sentiment);

            return Ok(products.Select(p => new ProductDto
            {
                Id = p.Id,
                ExternalId = p.ExternalId,
                Name = p.Name,
                Supplier = p.Supplier,
                ImageUrl = p.ImageUrl,
                SupplierPrice = p.SupplierPrice,
                EstimatedSalePrice = p.EstimatedSalePrice,
                MarginPercent = p.MarginPercent,
                Rating = p.Rating,
                Orders = p.Orders,
                CompetitionLevel = p.CompetitionLevel,
                Sentiment = p.Sentiment,
                Score = p.Score
            }).ToList());
        }

        // GET /api/products/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            var p = await _productRepo.GetByIdAsync(id);
            if (p == null) return NotFound();

            return Ok(new ProductDto
            {
                Id = p.Id,
                ExternalId = p.ExternalId,
                Name = p.Name,
                Supplier = p.Supplier,
                ImageUrl = p.ImageUrl,
                SupplierPrice = p.SupplierPrice,
                EstimatedSalePrice = p.EstimatedSalePrice,
                MarginPercent = p.MarginPercent,
                Rating = p.Rating,
                Orders = p.Orders,
                CompetitionLevel = p.CompetitionLevel,
                Sentiment = p.Sentiment,
                Score = p.Score
            });
        }

        // GET /api/products/hot?keyword=phone&categoryIds=1,2&minSalePrice=10&maxSalePrice=100&pageNo=1&pageSize=20&sort=default&platformProductType=ALL
        [HttpGet("hot")]
        public async Task<IActionResult> GetHotProducts([FromQuery] HotProductsFilterDto filter)
        {
            var products = await _hotProductsService.GetHotProductsAsync(filter);
            return Ok(products);
        }
    }
}
