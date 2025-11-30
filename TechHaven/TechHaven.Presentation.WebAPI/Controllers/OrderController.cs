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

[Authorize] // Yêu cầu phải đăng nhập mới thao tác được Order
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
    /// Helper: Lấy UserId từ Access Token (Claims)
    /// </summary>
    private int GetUserIdFromToken()
    {
        // Tùy thuộc vào cách bạn config JWT, Claim type có thể là "sub", "uid", hoặc ClaimTypes.NameIdentifier
        // Ở đây tôi ví dụ lấy theo NameIdentifier (thường là chuẩn)
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");

        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        {
            return userId;
        }

        // Nếu không lấy được (ví dụ dev mode tắt auth), trả về 0 hoặc throw exception tùy policy
        _logger.LogWarning("Could not extract UserID from Token. Defaulting to 0.");
        return 0;
    }

    /// <summary>
    /// GET api/Order
    /// Lấy danh sách đơn hàng có phân trang và lọc
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ResponseWrapper<PagingResponse<OrderDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrders(
        [FromQuery] OrderListQueryDto queryDto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Getting orders - Page: {PageNumber}/{PageSize}, Status: {Status}",
            queryDto.PageNumber, queryDto.PageSize, queryDto.Status);

        // Nếu muốn user chỉ xem được đơn của chính mình, hãy gán UserId vào queryDto tại đây
        // queryDto.UserId = GetUserIdFromToken();

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
    /// Lấy chi tiết một đơn hàng
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
            _logger.LogInformation("Order found: {OrderId}", result.Data?.OrderId);
        }
        else
        {
            _logger.LogWarning("Order not found: {OrderId}", id);
        }

        return HandleResult(result);
    }

    /// <summary>
    /// POST api/Order
    /// Tạo đơn hàng mới
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ResponseWrapper<OrderDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder(
        [FromBody] OrderUpsertRequestDto request,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetUserIdFromToken();
        _logger.LogInformation("User {UserId} is creating a new order.", currentUserId);

        // Map DTO to Command
        var command = new CreateOrderCommand
        {
            UserId = currentUserId, // Lấy tự động
            CustomerId = request.CustomerId,
            Status = (Domain.Enums.OrderStatus)request.Status, // Thường tạo mới là Pending, nhưng map theo request nếu cần
            Discount = request.Discount,
            Notes = request.Notes,
            // Map danh sách items từ DTO sang Command
            Details = request.Items ?? new List<OrderUpsertItemDto>()
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Order created successfully: ID {OrderId}", result.Data?.OrderId);
            // Dùng Object Initializer thay vì Constructor
            var response = new ResponseWrapper<OrderDto>
            {
                Success = true,
                Data = result.Data,
                Message = "Order created successfully"
            };

            return StatusCode(StatusCodes.Status201Created, response);
        }

        _logger.LogWarning("Failed to create order. Error: {ErrorMessage}", result.ErrorMessage);
        return HandleResult(result);
    }

    /// <summary>
    /// PUT api/Order/{id}
    /// Cập nhật đơn hàng (Thông tin chung + Thêm/Bớt sản phẩm)
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
        _logger.LogInformation("Updating order ID: {OrderId}", id);

        // Map DTO to Command
        var command = new UpdateOrderCommand
        {
            OrderId = id, // Lấy từ URL
            CustomerId = request.CustomerId,
            Status = (Domain.Enums.OrderStatus) request.Status,
            Discount = request.Discount,
            Notes = request.Notes,
            Details = request.Items ?? new List<OrderUpsertItemDto>()
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Order updated successfully: ID {OrderId}", id);
        }

        return HandleResult(result);
    }

    /// <summary>
    /// DELETE api/Order/{id}
    /// Xóa đơn hàng (chỉ cho phép khi Pending/Cancelled)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ResponseWrapper<bool>), StatusCodes.Status200OK)] // Hoặc 204 No Content tùy style
    [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteOrder(
        int id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting order ID: {OrderId}", id);

        var command = new DeleteOrderCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Order deleted successfully: ID {OrderId}", id);
        }
        return HandleResult(result);
    }
}