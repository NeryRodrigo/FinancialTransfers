using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialTransfers.Domain.Entities
{
    public enum PaymentStatus { Evaluating, Accepted, Denied }

    public class Payment
    {
        public Guid ExternalOperationId { get; private set; }
        public Guid CustomerId { get; private set; }
        public Guid ServiceProviderId { get; private set; }
        public int PaymentMethodId { get; private set; }
        public decimal Amount { get; private set; }
        public PaymentStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        private Payment() { }

        public Payment(Guid customerId, Guid serviceProviderId, int paymentMethodId, decimal amount)
        {
            ExternalOperationId = Guid.NewGuid();
            CustomerId = customerId;
            ServiceProviderId = serviceProviderId;
            PaymentMethodId = paymentMethodId;
            Amount = amount;
            Status = PaymentStatus.Evaluating;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateStatus(PaymentStatus newStatus)
        {
            Status = newStatus;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
