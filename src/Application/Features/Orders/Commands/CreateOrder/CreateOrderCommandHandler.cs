using CleanArchitectureCqrsTemplate.Application.Common;
using CleanArchitectureCqrsTemplate.Application.Interfaces;
using CleanArchitectureCqrsTemplate.Domain.Entities;
using CleanArchitectureCqrsTemplate.Domain.ValueObjects;
using MediatR;

namespace CleanArchitectureCqrsTemplate.Application.Features.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    private readonly IOrderRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrderCommandHandler(IOrderRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var order = new Order(request.CustomerName);

            foreach (var item in request.Items)
            {
                var orderItem = new OrderItem(
                    Guid.NewGuid(),
                    item.ProductName,
                    item.Quantity,
                    new Money(item.UnitPrice, "USD"));

                order.AddItem(orderItem);
            }

            await _repository.AddAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(order.Id);
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>($"Failed to create order: {ex.Message}");
        }
    }
}
