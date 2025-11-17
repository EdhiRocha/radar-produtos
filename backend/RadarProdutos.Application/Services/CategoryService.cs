using Microsoft.Extensions.Logging;
using RadarProdutos.Application.DTOs;
using RadarProdutos.Infrastructure.ExternalServices;

namespace RadarProdutos.Application.Services;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetCategoriesAsync();
}

public class CategoryService : ICategoryService
{
    private readonly IAliExpressClient _aliClient;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(IAliExpressClient aliClient, ILogger<CategoryService> logger)
    {
        _aliClient = aliClient;
        _logger = logger;
    }

    public async Task<List<CategoryDto>> GetCategoriesAsync()
    {
        var response = await _aliClient.GetCategoriesAsync();

        if (response == null ||
            response.AliexpressAffiliateCategoryGetResponse?.RespResult?.Result?.Categories?.Category == null ||
            !response.AliexpressAffiliateCategoryGetResponse.RespResult.Result.Categories.Category.Any())
        {
            _logger.LogWarning("Nenhuma categoria retornada pela API AliExpress");
            return new List<CategoryDto>();
        }

        var categories = response.AliexpressAffiliateCategoryGetResponse.RespResult.Result.Categories.Category
            .Select(c => new CategoryDto
            {
                CategoryId = c.CategoryId.ToString(),
                CategoryName = c.CategoryName ?? "Categoria sem nome",
                ParentCategoryId = c.ParentCategoryId?.ToString()
            })
            .ToList();

        return categories;
    }
}
