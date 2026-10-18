using CleanArchitectureCqrsTemplate.Application.Features.Orders.Commands.CreateOrder;
using CleanArchitectureCqrsTemplate.Application.Interfaces;
using CleanArchitectureCqrsTemplate.Domain.Entities;
using Xunit;

namespace CleanArchitectureCqrsTemplate.Application.Tests;

public sealed class FakeOrderRepository : IOrderRepository
{
    private readonly List<Order> _orders = new();

    public IReadOnlyList<Order> Orders => _orders;

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_orders.FirstOrDefault(o => o.Id == id));
    }

    public Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        _orders.Add(order);
        return Task.CompletedTask;
    }
}

public sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveChangesCallCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCallCount++;
        return Task.FromResult(1);
    }
}

public sealed class CreateOrderCommandHandlerTests
{
    private static CreateOrderCommand ValidCommand() => new(
        "Acme Corp",
        new List<CreateOrderItem>
        {
            new("Widget", 2, 9.99m),
            new("Gadget", 1, 24.50m)
        });

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithOrderId()
    {
        var repository = new FakeOrderRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new CreateOrderCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesOrderToRepository()
    {
        var repository = new FakeOrderRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new CreateOrderCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(repository.Orders);

        var saved = repository.Orders[0];
        Assert.Equal(result.Value, saved.Id);
        Assert.Equal("Acme Corp", saved.CustomerName);
        Assert.Equal(2, saved.Items.Count);
        Assert.Equal(2 * 9.99m + 1 * 24.50m, saved.Total);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }
}

public sealed class CreateOrderCommandValidatorTests
{
    private readonly CreateOrderCommandValidator _validator = new();

    [Fact]
    public void Validate_EmptyCustomerName_Fails()
    {
        var command = new CreateOrderCommand(
            string.Empty,
            new List<CreateOrderItem> { new("Widget", 1, 9.99m) });

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateOrderCommand.CustomerName));
    }

    [Fact]
    public void Validate_EmptyItems_Fails()
    {
        var command = new CreateOrderCommand("Acme Corp", new List<CreateOrderItem>());

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateOrderCommand.Items));
    }

    [Fact]
    public void Validate_InvalidItem_Fails()
    {
        var command = new CreateOrderCommand(
            "Acme Corp",
            new List<CreateOrderItem> { new(string.Empty, 0, -1m) });

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_ValidCommand_Passes()
    {
        var command = new CreateOrderCommand(
            "Acme Corp",
            new List<CreateOrderItem> { new("Widget", 2, 9.99m) });

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }
}
