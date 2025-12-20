using FluentValidation;
using TechHaven.Application.Features.Order.Commands.CreateOrder;
using TechHaven.Domain.Enums; // Đảm bảo đã có enum Draft

namespace TechHaven.Application.Features.Order.Commands.CreateOrder;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        // ---------------------------------------------------------
        // PHẦN 1: VALIDATION CƠ BẢN (Luôn áp dụng cho Pending)
        // ---------------------------------------------------------

        // Nếu có nhập CustomerId thì phải > 0
        RuleFor(x => x.CustomerId)
            .GreaterThan(0)
            .When(x => x.CustomerId.HasValue)
            .WithMessage("CustomerId must be valid.");

        // Discount không được âm
        RuleFor(x => x.Discount)
            .GreaterThanOrEqualTo(0).WithMessage("Discount cannot be negative.");

        // Nếu có nhập sản phẩm, thì sản phẩm đó phải hợp lệ (kể cả là Draft)
        // Dùng When(x => x.Details != null) để tránh lỗi null reference khi list rỗng
        RuleForEach(x => x.Details).ChildRules(items =>
        {
            items.RuleFor(i => i.ProductId)
                .GreaterThan(0).WithMessage("ProductId must be greater than 0.");

            items.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.");
        }).When(x => x.Details != null && x.Details.Count > 0);


        // ---------------------------------------------------------
        // PHẦN 2: VALIDATION CHẶT CHẼ (Chỉ áp dụng khi CHÍNH THỨC TẠO ĐƠN)
        // Khi Status != Pending, bắt buộc phải đủ thông tin
        // ---------------------------------------------------------

        When(x => x.Status != OrderStatus.Pending, () =>
        {
            // 1. Bắt buộc phải chọn khách hàng
            RuleFor(x => x.CustomerId)
                .NotNull().WithMessage("Customer is required for official orders.")
                .GreaterThan(0).WithMessage("CustomerId must be valid.");

            // 2. Bắt buộc phải có ít nhất 1 sản phẩm
            RuleFor(x => x.Details)
                .NotEmpty().WithMessage("Order must contain at least one item.")
                .Must(details => details != null && details.Count > 0)
                .WithMessage("Order detail list cannot be empty.");
        });
    }
}