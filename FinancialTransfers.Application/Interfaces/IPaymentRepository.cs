using FinancialTransfers.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialTransfers.Application.Interfaces
{
    public interface IPaymentRepository
    {
        Task AddAsync(Payment payment, CancellationToken ct = default);
        Task<Payment?> GetByExternalIdAsync(Guid externalId, CancellationToken ct = default);
        Task UpdateAsync(Payment payment, CancellationToken ct = default);
        Task<decimal> GetTotalAmountByCustomerTodayAsync(Guid customerId, CancellationToken ct = default);
    }
}
