using FinancialTransfers.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace FinancialTransfers.Application.Services
{
    public class RiskService : IRiskService
    {
        private readonly IPaymentRepository _repository;
        private readonly IKafkaProducer _kafkaProducer;
        private readonly ILogger<RiskService> _logger;

        public RiskService(
            IPaymentRepository repository,
            IKafkaProducer kafkaProducer,
            ILogger<RiskService> logger)
        {
            _repository = repository;
            _kafkaProducer = kafkaProducer;
            _logger = logger;
        }

        public async Task ProcessRiskEvaluationAsync(RiskEvaluationRequest request, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("[RiskEngine] Evaluando riesgo para Operación: {ExternalOperationId}", request.ExternalOperationId);

                string status = "accepted";

                if (request.Amount > 20000000m)
                {
                    status = "denied";
                    _logger.LogWarning("[RiskEngine] Operación {Id} RECHAZADA: Supera monto individual de 20M.", request.ExternalOperationId);
                }
                else
                {
                    var dailyTotal = await _repository.GetTotalAmountByCustomerTodayAsync(request.CustomerId, ct);

                    if (dailyTotal + request.Amount > 5000000m)
                    {
                        status = "denied";
                        _logger.LogWarning("[RiskEngine] Operación {Id} RECHAZADA: Acumulado diario ({Total}) supera los 5M.",
                            request.ExternalOperationId, dailyTotal + request.Amount);
                    }
                }

                var response = new RiskEvaluationResponse(request.ExternalOperationId, status);
                await _kafkaProducer.PublishRiskResponseAsync(response, ct);

                _logger.LogInformation("[RiskEngine] Resultado enviado para {Id}: {Status}", request.ExternalOperationId, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RiskEngine] Error crítico evaluando riesgo para la operación {Id}", request.ExternalOperationId);
                throw; 
            }
        }
    }
}