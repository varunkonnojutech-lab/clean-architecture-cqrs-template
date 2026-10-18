using CleanArchitectureCqrsTemplate.Application.Common;
using CleanArchitectureCqrsTemplate.Application.Dtos;
using CleanArchitectureCqrsTemplate.Application.Interfaces;
using MediatR;

namespace CleanArchitectureCqrsTemplate.Application.Features.Orders.Queries.GetOrderById;

public sealed class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderDto>>
{
    private readonly IOrderRepository _repository;

    public GetOrderByIdQueryHandler(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<OrderDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (order is null)
            return Result.Failure<OrderDto>("Order not found.");

        return Result.Success(OrderDto.FromOrder(order));
    }
}
