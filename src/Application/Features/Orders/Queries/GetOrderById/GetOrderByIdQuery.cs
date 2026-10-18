using CleanArchitectureCqrsTemplate.Application.Common;
using CleanArchitectureCqrsTemplate.Application.Dtos;
using MediatR;

namespace CleanArchitectureCqrsTemplate.Application.Features.Orders.Queries.GetOrderById;

public sealed record GetOrderByIdQuery(Guid Id) : IRequest<Result<OrderDto>>;
