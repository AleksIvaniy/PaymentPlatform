using PaymentPlatform.Domain.Enums;

namespace PaymentPlatform.Application.Dto;

public sealed record CreatePaymentResult(
    Guid PaymentId,
    PaymentStatus Status);