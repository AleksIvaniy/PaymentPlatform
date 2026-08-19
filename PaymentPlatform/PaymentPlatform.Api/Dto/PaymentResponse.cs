using PaymentPlatform.Domain.Enums;

namespace PaymentPlatform.Api.Dto;

public sealed record PaymentResponse(
    Guid PaymentId,
    PaymentStatus Status);