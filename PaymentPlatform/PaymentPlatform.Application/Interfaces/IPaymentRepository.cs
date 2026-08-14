using PaymentPlatform.Domain.Entities;

namespace PaymentPlatform.Application.Interfaces;

public interface IPaymentRepository
{
    Task AddAsync(
        Payment payment,
        CancellationToken cancellationToken = default);
}