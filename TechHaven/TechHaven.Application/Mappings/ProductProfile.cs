using AutoMapper;
using TechHaven.Application.Features.Product.Commands.CreateProduct;
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
        CreateMap<ProductUpsertRequest, Product>()
            .ForMember(dest => dest.ProductId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.OrderDetails, opt => opt.Ignore());

        // Map CreateProductCommand -> Product
        CreateMap<CreateProductCommand, Product>()
            .ForMember(dest => dest.ProductId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.Now))
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore()); // Ignore UpdatedAt
    }
}