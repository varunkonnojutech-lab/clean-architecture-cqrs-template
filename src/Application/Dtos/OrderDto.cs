using CleanArchitectureCqrsTemplate.Domain.Entities;

namespace CleanArchitectureCqrsTemplate.Application.Dtos;

public sealed record OrderDto(
    Guid Id,
    string CustomerName,
    string Status,
    decimal Total,
    DateTimeOffset DateCreated,
    List<OrderItemDto> Items)
{
    public static OrderDto FromOrder(Order order) => new(
        order.Id,
        order.CustomerName,
        order.Status.ToString(),
        order.Total,
        order.DateCreated,
        order.Items
            .Select(i => new OrderItemDto(i.Id, i.ProductName, i.Quantity, i.UnitPrice.Amount, i.LineTotal))
            .ToList());
}

public sealed record OrderItemDto(
    Guid Id,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);
