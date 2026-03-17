using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialTransfers.Application.Interfaces
{
    public record RiskEvaluationRequest(
        Guid ExternalOperationId,
        Guid CustomerId,
        decimal Amount);

    public record RiskEvaluationResponse(
        Guid ExternalOperationId,
        string Status);

    public interface IRiskService
    {
        Task ProcessRiskEvaluationAsync(RiskEvaluationRequest request, CancellationToken ct = default);
    }
}
