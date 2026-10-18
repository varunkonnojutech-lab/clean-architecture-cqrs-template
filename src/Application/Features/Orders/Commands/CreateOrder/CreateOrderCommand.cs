using CleanArchitectureCqrsTemplate.Application.Common;
using MediatR;

namespace CleanArchitectureCqrsTemplate.Application.Features.Orders.Commands.CreateOrder;

public sealed record CreateOrderCommand(string CustomerName, List<CreateOrderItem> Items)
    : IRequest<Result<Guid>>;

public sealed record CreateOrderItem(string ProductName, int Quantity, decimal UnitPrice);
