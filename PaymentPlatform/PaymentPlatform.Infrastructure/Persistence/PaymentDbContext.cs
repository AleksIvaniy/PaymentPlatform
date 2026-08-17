using Microsoft.EntityFrameworkCore;
using PaymentPlatform.Domain.Entities;

namespace PaymentPlatform.Infrastructure.Persistence;

public class PaymentDbContext: DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<Payment> Payments => Set<Payment>();
    
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(PaymentDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

}