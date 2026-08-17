using Microsoft.Extensions.DependencyInjection;
using PaymentPlatform.Application.Contracts;
using PaymentPlatform.Application.Services;

namespace PaymentPlatform.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IPaymentService, PaymentService>();

        return services;
    }
}
