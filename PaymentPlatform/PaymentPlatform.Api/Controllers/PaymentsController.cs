using Microsoft.AspNetCore.Mvc;
using PaymentPlatform.Api.Dto;
using PaymentPlatform.Application.Contracts;
using PaymentPlatform.Application.Dto;

namespace PaymentPlatform.Api.Controllers;



[ApiController]
[Route("api/[controller]")]
public class PaymentsController(IPaymentService paymentService) : ControllerBase
{
    private readonly IPaymentService _paymentService = paymentService;


    [HttpPost]
    public async Task<ActionResult<CreatePaymentResult>> Create(
        CreatePaymentRequest request,
        CancellationToken cancellationToken
    )
    {
        var command = new CreatePaymentCommand(
            request.UserId,
            request.Amount);
        
        var result = await _paymentService.HandleAsync(command, cancellationToken);
        
        var response = new PaymentResponse(
            result.PaymentId,
            result.Status);
        
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CreatePaymentResult>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _paymentService.GetByIdAsync(id, cancellationToken);
        if (result is null)
            return NotFound();
        
        var response = new PaymentResponse(
            result.PaymentId,
            result.Status);
        
        return Ok(response);
    }
}