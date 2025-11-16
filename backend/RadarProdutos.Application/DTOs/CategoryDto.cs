namespace RadarProdutos.Application.DTOs;

public class CategoryDto
{
    public string CategoryId { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? ParentCategoryId { get; set; }
}
