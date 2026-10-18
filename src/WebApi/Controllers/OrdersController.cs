using CleanArchitectureCqrsTemplate.Application.Features.Orders.Commands.CreateOrder;
using CleanArchitectureCqrsTemplate.Application.Features.Orders.Queries.GetOrderById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureCqrsTemplate.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateOrderCommand(
            request.CustomerName,
            request.Items
                .Select(i => new CreateOrderItem(i.ProductName, i.Quantity, i.UnitPrice))
                .ToList());

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetOrderByIdQuery(id), cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }
}

public sealed record CreateOrderRequest(string CustomerName, List<CreateOrderRequestItem> Items);

public sealed record CreateOrderRequestItem(string ProductName, int Quantity, decimal UnitPrice);
