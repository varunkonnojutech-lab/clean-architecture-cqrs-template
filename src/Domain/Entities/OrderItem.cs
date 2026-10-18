using CleanArchitectureCqrsTemplate.Domain.ValueObjects;

namespace CleanArchitectureCqrsTemplate.Domain.Entities;

public class OrderItem
{
    // For EF Core.
    private OrderItem()
    {
        ProductName = string.Empty;
    }

    public OrderItem(Guid id, string productName, int quantity, Money unitPrice)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id is required.", nameof(id));

        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name is required.", nameof(productName));

        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than 0.");

        Id = id;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public Guid Id { get; private set; }
    public string ProductName { get; private set; }
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; }

    public decimal LineTotal => UnitPrice.Amount * Quantity;
}
