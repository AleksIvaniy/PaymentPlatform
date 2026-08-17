

namespace PaymentPlatform.Application.Dto;

public sealed record CreatePaymentCommand(
    Guid UserId,
    decimal Amount);