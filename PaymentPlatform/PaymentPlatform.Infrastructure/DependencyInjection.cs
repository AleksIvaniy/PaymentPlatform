using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentPlatform.Application.Interfaces;
using PaymentPlatform.Infrastructure.Persistence;
using PaymentPlatform.Infrastructure.Persistence.Repositories;

namespace PaymentPlatform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Connection string 'Postgres' was not found.");

        services.AddDbContext<PaymentDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IPaymentRepository, PaymentRepository>();

        return services;
    }
}
