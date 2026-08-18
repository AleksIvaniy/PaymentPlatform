using Microsoft.AspNetCore.Mvc;
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
        CreatePaymentCommand command,
        CancellationToken cancellationToken
    )
    {
        var result = await _paymentService.HandleAsync(command, cancellationToken);
        
        return Ok(result);
    }
}