using RadarProdutos.Application.DTOs;
using RadarProdutos.Infrastructure.ExternalServices;

namespace RadarProdutos.Application.Services;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default);
}

public class CategoryService : ICategoryService
{
    private readonly IAliExpressClient _aliClient;

    public CategoryService(IAliExpressClient aliClient)
    {
        _aliClient = aliClient;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var response = await _aliClient.GetCategoriesAsync();

        if (response?.AliexpressAffiliateCategoryGetResponse?.RespResult?.Result?.Categories?.Category == null ||
            !response.AliexpressAffiliateCategoryGetResponse.RespResult.Result.Categories.Category.Any())
        {
            Console.WriteLine("⚠️ Nenhuma categoria retornada pela API");
            return Array.Empty<CategoryDto>();
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
