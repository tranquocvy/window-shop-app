using AutoMapper;
using TechHaven.Domain.Entities;
using TechHaven.Shared.DTOs.Products;

namespace TechHaven.Application.Mappings;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        // Mapping từ Product entity sang ProductDto
        CreateMap<Product, ProductDto>();

        // Chỉ cần 1 mapping, xử lý CreatedAt/UpdatedAt trong Service
        CreateMap<ProductCreateUpdateDto, Product>()
            .ForMember(dest => dest.ProductId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.OrderDetails, opt => opt.Ignore());
    }
}