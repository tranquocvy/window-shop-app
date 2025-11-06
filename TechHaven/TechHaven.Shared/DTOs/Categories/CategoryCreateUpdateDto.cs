namespace TechHaven.Shared.DTOs.Categories;

public class CategoryCreateUpdateDto
{
    public string CategoryName { get; set; } = string.Empty;

    public string? Description { get; set; }
}