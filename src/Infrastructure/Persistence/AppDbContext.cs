using Microsoft.EntityFrameworkCore;
using OrderEntity = CleanArchitectureCqrsTemplate.Domain.Entities.Order;
using OrderItemEntity = CleanArchitectureCqrsTemplate.Domain.Entities.OrderItem;

namespace CleanArchitectureCqrsTemplate.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<OrderEntity> Orders => Set<OrderEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderEntity>(b =>
        {
            b.HasKey(o => o.Id);

            b.Property(o => o.CustomerName)
                .IsRequired()
                .HasMaxLength(200);

            b.Property(o => o.Status)
                .HasConversion<string>()
                .IsRequired();

            b.Property(o => o.DateCreated)
                .IsRequired();

            // Computed in the domain; not persisted.
            b.Ignore(o => o.Total);

            b.HasMany(o => o.Items)
                .WithOne()
                .HasForeignKey("OrderId")
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // Items is exposed as IReadOnlyList<T> backed by a private List<T> field.
            b.Navigation(o => o.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<OrderItemEntity>(b =>
        {
            b.HasKey(i => i.Id);

            b.Property(i => i.ProductName)
                .IsRequired()
                .HasMaxLength(200);

            // Money is a struct, so it is mapped as a complex type
            // (OwnsOne/owned types require reference types).
            b.ComplexProperty(i => i.UnitPrice, mb =>
            {
                mb.Property(m => m.Amount)
                    .HasColumnName("UnitPriceAmount")
                    .IsRequired();

                mb.Property(m => m.Currency)
                    .HasColumnName("UnitPriceCurrency")
                    .IsRequired()
                    .HasMaxLength(3);
            });

            // Computed in the domain; not persisted.
            b.Ignore(i => i.LineTotal);
        });
    }
}
