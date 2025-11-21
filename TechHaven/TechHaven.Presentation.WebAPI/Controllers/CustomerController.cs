using MediatR;
using Microsoft.AspNetCore.Mvc;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Customers;
using TechHaven.Application.Features.Customer.Queries.GetCustomers;
using TechHaven.Application.Features.Customer.Queries.GetCustomerById;
using TechHaven.Application.Features.Customer.Commands.CreateCustomer;
using TechHaven.Application.Features.Customer.Commands.UpdateCustomer;
using TechHaven.Application.Features.Customer.Commands.DeleteCustomer;
using TechHaven.Application.Common.Exceptions;
using TechHaven.Domain.SearchCriteria;

namespace TechHaven.Presentation.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
  private readonly IMediator _mediator;

  public CustomerController(IMediator mediator)
  {
    _mediator = mediator;
  }

  [HttpGet]
  [ProducesResponseType(typeof(ResponseWrapper<PagingResponse<CustomerDto>>), StatusCodes.Status200OK)]
  public async Task<ActionResult<ResponseWrapper<PagingResponse<CustomerDto>>>> GetCustomers(
    [FromQuery] CustomerQueryDto queryDto,
    CancellationToken cancellationToken = default)
  {
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
    return Ok(new ResponseWrapper<PagingResponse<CustomerDto>>
    {
      Success = true,
      Message = "Customers retrieved successfully.",
      Data = result
    });
  }

  /// <summary>
  /// Get a single Customer by ID
  /// </summary>
  [HttpGet("{id}")]
  [ProducesResponseType(typeof(ResponseWrapper<CustomerDto>), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status404NotFound)]
  public async Task<ActionResult<ResponseWrapper<CustomerDto>>> GetCustomerById(
      int id,
      CancellationToken cancellationToken)
  {
    try
    {
      var query = new GetCustomerByIdQuery(id);
      var result = await _mediator.Send(query, cancellationToken);
      return Ok(new ResponseWrapper<CustomerDto>
      {
        Success = true,
        Message = "Customer retrieved successfully.",
        Data = result
      });
    }
    catch (NotFoundException ex)
    {
      return NotFound(new ResponseWrapper<object>
      {
        Success = false,
        Message = ex.Message
      });
    }
  }

  /// <summary>
  /// Create a new Customer
  /// </summary>
  [HttpPost]
  [ProducesResponseType(typeof(ResponseWrapper<CustomerDto>), StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status400BadRequest)]
  public async Task<ActionResult<ResponseWrapper<CustomerDto>>> CreateCustomer(
      [FromBody] CustomerCreateUpdateDto request,
      CancellationToken cancellationToken)
  {
    try
    {
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
      return CreatedAtAction(
        nameof(GetCustomerById),
        new { id = result.CustomerId },
        new ResponseWrapper<CustomerDto>
        {
          Success = true,
          Message = "Customer created successfully.",
          Data = result
        });
    }
    catch (Exception ex)
    {
      return BadRequest(new ResponseWrapper<object>
      {
        Success = false,
        Message = "Failed to create Customer.",
        Errors = new List<string> { ex.Message }
      });
    }
  }

  /// <summary>
  /// Update an existing Customer
  /// </summary>
  [HttpPut("{id}")]
  [ProducesResponseType(typeof(ResponseWrapper<CustomerDto>), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status404NotFound)]
  public async Task<ActionResult<ResponseWrapper<CustomerDto>>> UpdateCustomer(
      int id,
      [FromBody] CustomerCreateUpdateDto request,
      CancellationToken cancellationToken)
  {
    try
    {
      // Map DTO to Command with ID from route
      var command = new UpdateCustomerCommand
      {
        CustomerName = request.CustomerName,
        PhoneNumber = request.PhoneNumber,
        Email = request.Email,
        Address = request.Address,
        Type = request.Type,
        Note = request.Note,
      };

      var result = await _mediator.Send(command, cancellationToken);
      return Ok(new ResponseWrapper<CustomerDto>
      {
        Success = true,
        Message = "Customer updated successfully.",
        Data = result
      });
    }
    catch (NotFoundException ex)
    {
      return NotFound(new ResponseWrapper<object>
      {
        Success = false,
        Message = ex.Message
      });
    }
  }

  /// <summary>
  /// Delete a Customer
  /// </summary>
  [HttpDelete("{id}")]
  [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status404NotFound)]
  public async Task<ActionResult<ResponseWrapper<object>>> DeleteCustomer(
      int id,
      CancellationToken cancellationToken)
  {
    try
    {
      var command = new DeleteCustomerCommand(id);
      await _mediator.Send(command, cancellationToken);
      return NoContent();
    }
    catch (NotFoundException ex)
    {
      return NotFound(new ResponseWrapper<object>
      {
        Success = false,
        Message = ex.Message
      });
    }
  }
}