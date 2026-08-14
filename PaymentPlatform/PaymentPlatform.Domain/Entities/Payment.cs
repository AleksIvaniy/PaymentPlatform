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
        if(userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty", nameof(userId));

        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount),"Payment amount must be greater than zero");
        
        Id = Guid.NewGuid();
        UserId = userId;
        Amount = amount;
        Status = PaymentStatus.Pending;
        CreatedAt = DateTime.UtcNow;

    }

    public void MarkAsProcessing()
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Payment cannot be moved to Processing from {Status}.");
        
        Status = PaymentStatus.Processing;
    }

    public void Complete()
    {
        if (Status != PaymentStatus.Processing)
            throw new InvalidOperationException($"Payment cannot be completed from {Status}.");
        
        Status = PaymentStatus.Completed;
    }

    public void Fail()
    {
        if(Status == PaymentStatus.Completed)
            throw new InvalidOperationException("Completed payment cannot be failed.");
        
        Status = PaymentStatus.Failed;
    }
}