using FluentValidation;
using PaymentPlatform.Api.Dto;

namespace PaymentPlatform.Api.Validation;

public sealed class CreatePaymentRequestValidator: AbstractValidator<CreatePaymentRequest>
{
    public CreatePaymentRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId must not be empty.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero.");
    }
}