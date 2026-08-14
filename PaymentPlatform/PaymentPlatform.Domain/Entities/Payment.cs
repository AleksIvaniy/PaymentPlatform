using PaymentPlatform.Domain.Enums;

namespace PaymentPlatform.Domain.Entities;

public class Payment
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public decimal Amount { get; private set; }

    public PaymentStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    private Payment()
    {
    }

    public Payment(Guid userId, decimal amount)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Amount = amount;
        Status = PaymentStatus.Pending;

    }
}