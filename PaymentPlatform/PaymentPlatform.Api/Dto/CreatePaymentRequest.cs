namespace PaymentPlatform.Api.Dto;

public sealed record CreatePaymentRequest(
    Guid UserId,
    decimal Amount);