using FluentValidation;
//using TechHaven.Domain.Interfaces;

namespace TechHaven.Application.Features.Order.Commands.UpdateOrder;

public class UpdateOrderCommandValidator : AbstractValidator<UpdateOrderCommand>
{
    public UpdateOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .GreaterThan(0).WithMessage("Order ID must be valid.");

        // Rule Discount % [0, 1]
        RuleFor(x => x.Discount)
             .GreaterThanOrEqualTo(0).WithMessage("Discount cannot be negative.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid order status.");

        RuleFor(x => x.Details)
            .NotEmpty().WithMessage("Order must contain at least one item.");

        RuleForEach(x => x.Details).ChildRules(items =>
        {
            items.RuleFor(i => i.ProductId).GreaterThan(0);
            items.RuleFor(i => i.Quantity).GreaterThan(0);
        });
    }
}