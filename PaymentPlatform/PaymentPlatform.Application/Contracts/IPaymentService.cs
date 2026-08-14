using PaymentPlatform.Application.Dto;

namespace PaymentPlatform.Application.Contracts;

public interface IPaymentService
{
    public Task<CreatePaymentResult> HandleAsync(CreatePaymentCommand command,
        CancellationToken cancellationToken = default);
}