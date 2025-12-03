using FluentValidation;
using TechHaven.Application.Features.Order.Commands.CreateOrder; // namespace chứa command
using TechHaven.Domain.Interfaces;

namespace TechHaven.Application.Features.Order.Commands.CreateOrder;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrderCommandValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        //  Kiểm tra thông tin chung
        RuleFor(x => x.CustomerId)
            .GreaterThan(0).When(x => x.CustomerId.HasValue)
            .WithMessage("CustomerId must be valid.");

        //  Kiểm tra danh sách sản phẩm (Bắt buộc phải có hàng mới tạo đơn được)
        RuleFor(x => x.Details)
            .NotEmpty().WithMessage("Order must contain at least one item.")
            .Must(details => details != null && details.Count > 0)
            .WithMessage("Order detail list cannot be empty.");

        //  Validate từng item trong danh sách (Nested Validation)
        RuleForEach(x => x.Details).ChildRules(items =>
        {
            items.RuleFor(i => i.ProductId)
                .GreaterThan(0).WithMessage("ProductId must be greater than 0.");

            items.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.");
        });

        //Validate Discount
        RuleFor(x => x.Discount)
            .GreaterThanOrEqualTo(0).WithMessage("Discount cannot be negative.")
            .LessThanOrEqualTo(1).WithMessage("Discount cannot exceed 100%.");

        // không validate UnitPrice ở đây vì ta sẽ lấy giá từ DB trong Handler
    }
}