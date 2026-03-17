using FinancialTransfers.Application.Interfaces;
using FinancialTransfers.Application.Payments;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FinancialTransfers.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IPaymentRepository _repository;

        public PaymentsController(IMediator mediator, IPaymentRepository repository)
        {
            _mediator = mediator;
            _repository = repository;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentCommand command)
        {
            var operationId = await _mediator.Send(command);
            return Ok(new { externalOperationId = operationId });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStatus(Guid id)
        {
            var payment = await _repository.GetByExternalIdAsync(id, default);
            if (payment == null) return NotFound();

            return Ok(new
            {
                payment.ExternalOperationId,
                payment.CreatedAt,
                status = payment.Status.ToString().ToLower()
            });
        }
    }
}
