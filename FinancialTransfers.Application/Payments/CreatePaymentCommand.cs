using FinancialTransfers.Application.Interfaces;
using FinancialTransfers.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialTransfers.Application.Payments
{
    public record CreatePaymentCommand(Guid CustomerId, Guid ServiceProviderId, int PaymentMethodId, decimal Amount) : IRequest<Guid>;

    public class CreatePaymentHandler : IRequestHandler<CreatePaymentCommand, Guid>
    {
        private readonly IPaymentRepository _repository;
        private readonly IKafkaProducer _producer;

        public CreatePaymentHandler(IPaymentRepository repository, IKafkaProducer producer)
        {
            _repository = repository;
            _producer = producer;
        }

        public async Task<Guid> Handle(CreatePaymentCommand request, CancellationToken ct)
        {
            var payment = new Payment(request.CustomerId, request.ServiceProviderId, request.PaymentMethodId, request.Amount);

            await _repository.AddAsync(payment, ct);

            await _producer.PublishRiskRequestAsync(new
            {
                payment.ExternalOperationId,
                payment.CustomerId,
                payment.Amount
            }, ct);

            return payment.ExternalOperationId;
        }
    }
}
