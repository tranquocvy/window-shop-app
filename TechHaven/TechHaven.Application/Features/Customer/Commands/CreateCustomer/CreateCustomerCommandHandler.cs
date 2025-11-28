using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.Customers;
using AutoMapper;
using TechHaven.Domain.Interfaces;
using TechHaven.Domain.Common;
using TechHaven.Shared.DTOs.Products;

namespace TechHaven.Application.Features.Customer.Commands.CreateCustomer;

public class CreateCustomerCommandHandler : ICommandHandler<CreateCustomerCommand, Result<CustomerDto>>
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly IMapper _mapper;

  public CreateCustomerCommandHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper)
  {
    _unitOfWork = unitOfWork;
    _mapper = mapper;
  }

  public async Task<Result<CustomerDto>> Handle(
    CreateCustomerCommand request,
    CancellationToken cancellationToken)
  {
    try
    {
      var existingCustomer = await _unitOfWork.Customers.FirstOrDefaultAsync(p =>
      p.PhoneNumber == request.PhoneNumber,
      cancellationToken
    );

      if (existingCustomer != null)
      {
        return Result<CustomerDto>.Failure(
          $"Customer with name '{request.CustomerName}' and email '{request.Email}' already exists",
          ErrorType.Conflict
        );
      }

      // map command to entity
      var customer = _mapper.Map<Domain.Entities.Customer>(request);

      // set timestamps with UTC for PostgreSQL compatibility
      customer.CreatedAt = DateTime.UtcNow;
      customer.UpdatedAt = null;

      // add to repository
      await _unitOfWork.Customers.AddAsync(customer, cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);

      var customerDto = _mapper.Map<CustomerDto>(customer);

      return Result<CustomerDto>.Success(customerDto);
    }
    catch (Exception ex)
    {
      var innerMessage = ex.InnerException?.Message ?? ex.Message;
      return Result<CustomerDto>.Failure(
        $"Failed to create customer: {innerMessage}",
        ErrorType.InternalError);
    }
  }
}