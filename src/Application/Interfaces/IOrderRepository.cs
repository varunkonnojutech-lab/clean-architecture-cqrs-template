using CleanArchitectureCqrsTemplate.Domain.Entities;

namespace CleanArchitectureCqrsTemplate.Application.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(Order order, CancellationToken cancellationToken = default);
}
