using PaymentPlatform.Application.Contracts;
using PaymentPlatform.Application.Dto;
using PaymentPlatform.Application.Exceptions;
using PaymentPlatform.Application.Interfaces;
using PaymentPlatform.Domain.Entities;

namespace PaymentPlatform.Application.Services;

public sealed class PaymentService: IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;

    public PaymentService(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<CreatePaymentResult> HandleAsync(
        CreatePaymentCommand command,
        CancellationToken cancellationToken = default)
    {
        
        //throw new Exception("Test exception");
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

    public async Task<CreatePaymentResult> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(id, cancellationToken);
        if (payment is null)
        {
            throw new NotFoundException(
                $"Payment with id '{id}' was not found.");
        }
        
        return new CreatePaymentResult(payment.Id, payment.Status);
    }
}