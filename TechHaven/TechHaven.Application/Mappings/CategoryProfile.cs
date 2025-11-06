using AutoMapper;
using TechHaven.Domain.Entities;
using TechHaven.Shared.DTOs.Categories;

namespace TechHaven.Application.Mappings;

public class CategoryProfile : Profile
{
	public CategoryProfile()
	{
		CreateMap<Category, CategoryDto>()
			.ForMember(dest => dest.ProductsCount,
				opt => opt.MapFrom(src => src.Products != null ? src.Products.Count : 0));

		CreateMap<CategoryCreateUpdateDto, Category>()
			.ForMember(dest => dest.CategoryId, opt => opt.Ignore())
			.ForMember(dest => dest.Products, opt => opt.Ignore());
	}
}