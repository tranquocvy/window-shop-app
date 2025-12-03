using FluentValidation;
using TechHaven.Domain.Interfaces;
using TechHaven.Application.Features.Customer.Commands.CreateCustomer;

namespace TechHaven.Application.Features.Customer.Commands.CreateCustomer;

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Customer name is required.")
            .MaximumLength(150).WithMessage("Customer name cannot exceed 150 characters.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .MaximumLength(15).WithMessage("Phone number cannot exceed 15 characters.")
            .Matches(@"^[\d\-\+\(\)\s]+$").WithMessage("Phone number contains invalid characters.");

        RuleFor(x => x.Email)
            .MaximumLength(150).WithMessage("Email address cannot exceed 150 characters.")
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email)).WithMessage("Invalid email address format.");

        RuleFor(x => x.Address)
            .MaximumLength(300).WithMessage("Address cannot exceed 300 characters.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Customer type is required.");

        RuleFor(x => x.Note)
            .MaximumLength(255).WithMessage("Note cannot exceed 255 characters.");
    }
}