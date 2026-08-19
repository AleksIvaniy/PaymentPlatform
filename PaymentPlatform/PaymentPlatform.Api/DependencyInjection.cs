using FluentValidation;
using PaymentPlatform.Api.Validation;

namespace PaymentPlatform.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(
        this IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddValidatorsFromAssemblyContaining<
            CreatePaymentRequestValidator>();

        return services;
    }
}