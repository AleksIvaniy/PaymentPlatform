using PaymentPlatform.Application.Dto;

namespace PaymentPlatform.Application.Contracts;

public interface IPaymentService
{
    Task<CreatePaymentResult> HandleAsync(CreatePaymentCommand command,
        CancellationToken cancellationToken = default);
    Task<CreatePaymentResult> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}