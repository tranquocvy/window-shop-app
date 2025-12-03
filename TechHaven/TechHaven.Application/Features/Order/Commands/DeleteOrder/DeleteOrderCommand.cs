using TechHaven.Application.Interfaces;
using TechHaven.Domain.Common;

namespace TechHaven.Application.Features.Order.Commands.DeleteOrder;

public record DeleteOrderCommand(int OrderId) : ICommand<Result<bool>>;