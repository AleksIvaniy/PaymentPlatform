using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using PaymentPlatform.Api.Dto;
using PaymentPlatform.Application.Contracts;
using PaymentPlatform.Application.Dto;

namespace PaymentPlatform.Api.Controllers;



[ApiController]
[Route("api/[controller]")]
public class PaymentsController(IPaymentService paymentService, IValidator<CreatePaymentRequest> validator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CreatePaymentResult>> Create(
        CreatePaymentRequest request,
        CancellationToken cancellationToken
    )
    {
        
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToDictionary());
        }
        
        var command = new CreatePaymentCommand(
            request.UserId,
            request.Amount);
        
        var result = await paymentService.HandleAsync(command, cancellationToken);
        
        var response = new PaymentResponse(
            result.PaymentId,
            result.Status);
        
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CreatePaymentResult>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await paymentService.GetByIdAsync(id, cancellationToken);
        if (result is null)
            return NotFound();
        
        var response = new PaymentResponse(
            result.PaymentId,
            result.Status);
        
        return Ok(response);
    }
}