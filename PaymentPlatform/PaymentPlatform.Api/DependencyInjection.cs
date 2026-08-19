using FluentValidation;
using PaymentPlatform.Api.Exceptions;
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
        
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddValidatorsFromAssemblyContaining<
            CreatePaymentRequestValidator>();

        return services;
    }
}