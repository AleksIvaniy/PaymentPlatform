using Microsoft.EntityFrameworkCore;
using PaymentPlatform.Application.Interfaces;
using PaymentPlatform.Domain.Entities;

namespace PaymentPlatform.Infrastructure.Persistence.Repositories;

public class PaymentRepository: IPaymentRepository
{
    private readonly PaymentDbContext _dbContext;
    
    public PaymentRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    
    public async Task AddAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        await _dbContext.Payments.AddAsync(payment, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Payments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}