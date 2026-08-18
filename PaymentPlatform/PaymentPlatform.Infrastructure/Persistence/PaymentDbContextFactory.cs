using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PaymentPlatform.Infrastructure.Persistence;

public sealed class PaymentDbContextFactory : IDesignTimeDbContextFactory<PaymentDbContext>
{
    public PaymentDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("PAYMENT_DB_CONNECTION")
            ?? "Host=localhost;Port=5433;Database=payment_platform;Username=postgres;Password=AIaae2026$";

        var optionsBuilder = new DbContextOptionsBuilder<PaymentDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new PaymentDbContext(optionsBuilder.Options);
    }
}
