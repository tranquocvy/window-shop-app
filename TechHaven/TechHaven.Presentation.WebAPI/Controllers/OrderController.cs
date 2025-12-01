using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechHaven.Application.Features.Order.Commands.CreateOrder;
using TechHaven.Application.Features.Order.Commands.DeleteOrder;
using TechHaven.Application.Features.Order.Commands.UpdateOrder;
using TechHaven.Application.Features.Order.Queries.GetOrderById;
using TechHaven.Application.Features.Order.Queries.GetOrders;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Orders;

namespace TechHaven.Presentation.WebAPI.Controllers;

//[Authorize] -> Bỏ để cho phép truy cập công khai (nếu cần thiết)
public class OrderController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrderController> _logger;

    public OrderController(IMediator mediator, ILogger<OrderController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Helper: Lấy UserId từ Access Token
    /// </summary>
    private int GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");

        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        {
            return userId;
        }

        // Log warning nếu không lấy được UserID (có thể do cấu hình Token sai hoặc Auth middleware lỏng lẻo)
        _logger.LogWarning("Security Alert: Could not extract UserID from Token in OrderController.");
        return 0;
    }

    /// <summary>
    /// GET api/Order
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ResponseWrapper<PagingResponse<OrderDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrders(
        [FromQuery] OrderListQueryDto queryDto,
        CancellationToken cancellationToken = default)
    {
        // Log các tham số lọc để dễ debug khi client báo "tìm không thấy đơn"
        _logger.LogInformation(
            "Getting orders - Page: {PageNumber}/{PageSize}, Status: {Status}, Keyword: {Keyword}, DateRange: {From}-{To}",
            queryDto.PageNumber,
            queryDto.PageSize,
            queryDto.Status,
            queryDto.CustomerKeyword ?? "None",
            queryDto.OrderDate?.StartDate?.ToShortDateString() ?? "Any",
            queryDto.OrderDate?.EndDate?.ToShortDateString() ?? "Any"
            );

        var query = new GetOrdersQuery(queryDto);
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsSuccess)
        {
            _logger.LogInformation(
                "Retrieved {Count} orders (Total: {TotalCount})",
                result.Data?.Items.Count, result.Data?.TotalCount);
        }

        return HandleResult(result);
    }

    /// <summary>
    /// GET api/Order/{id}
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ResponseWrapper<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderById(
        int id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting order detail with ID: {OrderId}", id);

        var query = new GetOrderByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Order found: {OrderId} - Customer: {CustomerName}",
                result.Data?.OrderId, result.Data?.CustomerName);
        }
        else
        {
            _logger.LogWarning("Order lookup failed: ID {OrderId} not found.", id);
        }

        return HandleResult(result);
    }

    /// <summary>
    /// POST api/Order
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ResponseWrapper<OrderDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder(
        [FromBody] OrderUpsertRequestDto request,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetUserIdFromToken();

        // Log quan trọng: Ai đang cố tạo đơn, cho khách hàng nào, bao nhiêu món?
        _logger.LogInformation(
            "User {UserId} initiating order creation. CustomerId: {CustomerId}, ItemCount: {ItemCount}",
            currentUserId, request.CustomerId, request.Items?.Count ?? 0);

        var command = new CreateOrderCommand
        {
            UserId = currentUserId,
            CustomerId = request.CustomerId,
            Status = (Domain.Enums.OrderStatus)request.Status,
            Discount = request.Discount,
            Notes = request.Notes,
            // Map đúng kiểu dữ liệu như đã sửa ở bước trước
            Details = request.Items ?? new List<OrderUpsertItemDto>()
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            _logger.LogInformation(
                "Order created successfully: ID {OrderId}, TotalAmount: {TotalAmount}",
                result.Data?.OrderId, result.Data?.TotalAmount);

            var response = new ResponseWrapper<OrderDto>
            {
                Success = true,
                Data = result.Data,
                Message = "Order created successfully"
            };

            return StatusCode(StatusCodes.Status201Created, response);
        }

        // Log Warning khi tạo thất bại (ví dụ: Hết hàng, Validate sai)
        _logger.LogWarning(
            "Failed to create order for User {UserId}. Error: {ErrorMessage}",
            currentUserId, result.ErrorMessage);

        return HandleResult(result);
    }

    /// <summary>
    /// PUT api/Order/{id}
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ResponseWrapper<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOrder(
        int id,
        [FromBody] OrderUpsertRequestDto request,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetUserIdFromToken();

        _logger.LogInformation(
            "User {UserId} updating order ID: {OrderId}. New ItemCount: {ItemCount}",
            currentUserId, id, request.Items?.Count ?? 0);

        var command = new UpdateOrderCommand
        {
            OrderId = id,
            CustomerId = request.CustomerId,
            Status = (Domain.Enums.OrderStatus)request.Status,
            Discount = request.Discount,
            Notes = request.Notes,
            Details = request.Items ?? new List<OrderUpsertItemDto>()
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Order {OrderId} updated successfully.", id);
        }
        else
        {
            _logger.LogWarning("Failed to update order {OrderId}. Error: {ErrorMessage}", id, result.ErrorMessage);
        }

        return HandleResult(result);
    }

    /// <summary>
    /// DELETE api/Order/{id}
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ResponseWrapper<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteOrder(
        int id,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetUserIdFromToken();
        _logger.LogInformation("User {UserId} requesting to delete order ID: {OrderId}", currentUserId, id);

        var command = new DeleteOrderCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Order {OrderId} deleted successfully (Restocked inventory).", id);
        }
        else
        {
            _logger.LogWarning("Failed to delete order {OrderId}. Error: {ErrorMessage}", id, result.ErrorMessage);
        }

        return HandleResult(result);
    }
}