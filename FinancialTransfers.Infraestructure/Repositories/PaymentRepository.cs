using FinancialTransfers.Application.Interfaces;
using FinancialTransfers.Domain.Entities;
using FinancialTransfers.Infraestructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialTransfers.Infraestructure.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentRepository(ApplicationDbContext context) => _context = context;

        public async Task AddAsync(Payment payment, CancellationToken ct)
        {
            await _context.Payments.AddAsync(payment, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<Payment?> GetByExternalIdAsync(Guid externalId, CancellationToken ct)
        {
            return await _context.Payments.FirstOrDefaultAsync(p => p.ExternalOperationId == externalId, ct);
        }

        public async Task UpdateAsync(Payment payment, CancellationToken ct)
        {
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<decimal> GetTotalAmountByCustomerTodayAsync(Guid customerId, CancellationToken ct)
        {
            var today = DateTime.UtcNow.Date;
            return await _context.Payments
                .Where(p => p.CustomerId == customerId && p.CreatedAt >= today)
                .SumAsync(p => p.Amount, ct);
        }
    }
}
