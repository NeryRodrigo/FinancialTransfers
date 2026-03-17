using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialTransfers.Application.Interfaces
{
    public interface IKafkaProducer
    {
        Task PublishRiskRequestAsync(object message, CancellationToken ct = default);

        Task PublishRiskResponseAsync(RiskEvaluationResponse response, CancellationToken ct = default);
    }
}
