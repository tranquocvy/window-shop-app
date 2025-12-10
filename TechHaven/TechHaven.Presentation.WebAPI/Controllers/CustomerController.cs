using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Customers;
using TechHaven.Application.Features.Customer.Queries.GetCustomers;
using TechHaven.Application.Features.Customer.Queries.GetCustomerById;
using TechHaven.Application.Features.Customer.Commands.CreateCustomer;
using TechHaven.Application.Features.Customer.Commands.UpdateCustomer;
using TechHaven.Application.Features.Customer.Commands.DeleteCustomer;
using TechHaven.Application.Common.Exceptions;
using TechHaven.Domain.SearchCriteria;
using Microsoft.AspNetCore.Authorization;
using TechHaven.Infrastructure.Authorization;

namespace TechHaven.Presentation.WebAPI.Controllers;

[Authorize(Policy = AuthorizationPolicies.ManageCustomers)]
public class CustomerController : BaseApiController
{
  private readonly IMediator _mediator;
  private readonly ILogger<CustomerController> _logger;

  public CustomerController(IMediator mediator, ILogger<CustomerController> logger)
  {
    _mediator = mediator;
    _logger = logger;
  }

  [HttpGet]
  [ProducesResponseType(typeof(ResponseWrapper<PagingResponse<CustomerDto>>), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetCustomers(
    [FromQuery] CustomerListQueryDto queryDto,
    CancellationToken cancellationToken = default)
  {
    _logger.LogInformation(
      "Getting customers with SearchTerm: {SearchTerm}, Page: {PageNumber}/{PageSize}",
      queryDto.SearchTerm, queryDto.PageNumber, queryDto.PageSize);

    // Map CustomerQueryDto -> CustomerSearchCriteria
    var criteria = new CustomerSearchCriteria
    {
      SearchTerm = queryDto.SearchTerm,
      PageNumber = queryDto.PageNumber,
      PageSize = queryDto.PageSize,
      SortBy = queryDto.Sorting?.SortBy,
      SortDescending = queryDto.Sorting?.Desc ?? false,
    };

    var query = new GetCustomersQuery(criteria);

    var result = await _mediator.Send(query, cancellationToken);

    if (result.IsSuccess)
    {
      _logger.LogInformation(
        "Retrieved {Count} customers (Total: {TotalCount})",
        result.Data?.Items.Count, result.Data?.TotalCount);
    }

    return HandleResult(result);
  }

  /// <summary>
  /// Get a single Customer by ID
  /// </summary>
  [HttpGet("{id}")]
  [ProducesResponseType(typeof(ResponseWrapper<CustomerDto>), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status404NotFound)]
  public async Task<IActionResult> GetCustomerById(
      int id,
      CancellationToken cancellationToken)
  {
    _logger.LogInformation("Getting customer with ID: {CustomerId}", id);

    var query = new GetCustomerByIdQuery(id);
    var result = await _mediator.Send(query, cancellationToken);

    if (result.IsSuccess)
    {
      _logger.LogInformation("Customer found: {CustomerName}", result.Data?.CustomerName);
    }
    else
    {
      _logger.LogWarning("Customer not found: {CustomerId}", id);
    }

    return HandleResult(result);
  }

  /// <summary>
  /// Create a new Customer
  /// </summary>
  [HttpPost]
  [ProducesResponseType(typeof(ResponseWrapper<CustomerDto>), StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> CreateCustomer(
      [FromBody] CustomerUpsertRequestDto request,
      CancellationToken cancellationToken)
  {
    _logger.LogInformation(
      "Creating customer: {Customer}",
      request.CustomerName
    );

    // Map DTO to Command
    var command = new CreateCustomerCommand
    {
      CustomerName = request.CustomerName,
      PhoneNumber = request.PhoneNumber,
      Email = request.Email,
      Address = request.Address,
      Type = request.Type,
      Note = request.Note,
    };

    var result = await _mediator.Send(command, cancellationToken);

    if (result.IsSuccess)
    {
      _logger.LogInformation(
        "Customer created successfully: ID {CustomerId}, Name: {CustomerName}",
        result.Data?.CustomerId, result.Data?.CustomerName);
    }
    else
    {
      _logger.LogWarning(
        "Failed to create customer: {CustomerName}. Error: {ErrorMessage}",
        request.CustomerName, result.ErrorMessage);
    }

    return HandleResult(
      result,
      nameof(GetCustomerById),
      new { id = result.Data?.CustomerId }
    );
  }

  /// <summary>
  /// Update an existing Customer
  /// </summary>
  [HttpPut("{id}")]
  [ProducesResponseType(typeof(ResponseWrapper<CustomerDto>), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status404NotFound)]
  public async Task<IActionResult> UpdateCustomer(
      int id,
      [FromBody] CustomerUpsertRequestDto request,
      CancellationToken cancellationToken)
  {
    _logger.LogInformation(
      "Updating Customer ID: {CustomerId} - New name: {CustomerName}",
      id, request.CustomerName
    );

    // Map DTO to Command with ID from route
    var command = new UpdateCustomerCommand
    {
      CustomerId = id,
      CustomerName = request.CustomerName,
      PhoneNumber = request.PhoneNumber,
      Email = request.Email,
      Address = request.Address,
      Type = request.Type,
      Note = request.Note,
    };

    var result = await _mediator.Send(command, cancellationToken);

    if (result.IsSuccess)
    {
      _logger.LogInformation("Customer updated successfully: ID {CustomerId}", id);
    }

    return HandleResult(result);
  }

  /// <summary>
  /// Delete a Customer
  /// </summary>
  [HttpDelete("{id}")]
  [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status404NotFound)]
  public async Task<IActionResult> DeleteCustomer(
      int id,
      CancellationToken cancellationToken)
  {
    _logger.LogInformation("Deleting customer ID: {CustomerId}", id);

    var command = new DeleteCustomerCommand(id);
    var result = await _mediator.Send(command, cancellationToken);

    if (result.IsSuccess)
    {
      _logger.LogInformation("Customer deleted successfully: ID {CustomerId}", id);
    }

    return result.IsSuccess
      ? NoContent()
      : HandleResult(result);
  }
}