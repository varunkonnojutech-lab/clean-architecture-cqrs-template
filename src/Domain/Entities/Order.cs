using CleanArchitectureCqrsTemplate.Domain.Enums;

namespace CleanArchitectureCqrsTemplate.Domain.Entities;

public class Order
{
    private readonly List<OrderItem> _items = new();

    // For EF Core.
    private Order()
    {
        CustomerName = string.Empty;
    }

    public Order(string customerName)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("Customer name is required.", nameof(customerName));

        Id = Guid.NewGuid();
        CustomerName = customerName;
        Status = OrderStatus.Pending;
        DateCreated = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string CustomerName { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTimeOffset DateCreated { get; private set; }

    public IReadOnlyList<OrderItem> Items => _items;

    public decimal Total => _items.Sum(i => i.LineTotal);

    public void AddItem(OrderItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Items can only be added while the order is pending.");

        _items.Add(item);
    }

    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Only a pending order can be confirmed.");

        Status = OrderStatus.Confirmed;
    }

    public void Ship()
    {
        if (Status != OrderStatus.Confirmed)
            throw new InvalidOperationException("Only a confirmed order can be shipped.");

        Status = OrderStatus.Shipped;
    }

    public void Deliver()
    {
        if (Status != OrderStatus.Shipped)
            throw new InvalidOperationException("Only a shipped order can be delivered.");

        Status = OrderStatus.Delivered;
    }

    public void Cancel()
    {
        if (Status != OrderStatus.Pending && Status != OrderStatus.Confirmed)
            throw new InvalidOperationException("Only a pending or confirmed order can be cancelled.");

        Status = OrderStatus.Cancelled;
    }
}
