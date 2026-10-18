using CleanArchitectureCqrsTemplate.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using OrderEntity = CleanArchitectureCqrsTemplate.Domain.Entities.Order;

namespace CleanArchitectureCqrsTemplate.Infrastructure.Persistence.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<OrderEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task AddAsync(OrderEntity order, CancellationToken cancellationToken = default)
    {
        await _context.Orders.AddAsync(order, cancellationToken);
    }
}
