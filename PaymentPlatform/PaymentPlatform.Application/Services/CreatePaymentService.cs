using PaymentPlatform.Application.Dto;
using PaymentPlatform.Application.Interfaces;
using PaymentPlatform.Domain.Entities;

namespace PaymentPlatform.Application.Services;

public sealed class CreatePaymentHandler
{
    private readonly IPaymentRepository _paymentRepository;

    public CreatePaymentHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<CreatePaymentResult> HandleAsync(
        CreatePaymentCommand command,
        CancellationToken cancellationToken = default)
    {
        var payment = new Payment(
            command.UserId,
            command.Amount);

        await _paymentRepository.AddAsync(
            payment,
            cancellationToken);

        return new CreatePaymentResult(
            payment.Id,
            payment.Status);
    }
}