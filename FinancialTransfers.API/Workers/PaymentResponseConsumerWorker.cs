using Confluent.Kafka;
using FinancialTransfers.Application.Interfaces;
using System.Text.Json;

namespace FinancialTransfers.API.Workers
{
    public class PaymentResponseConsumerWorker : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PaymentResponseConsumerWorker> _logger;

        public PaymentResponseConsumerWorker(
            IConfiguration configuration,
            IServiceScopeFactory scopeFactory,
            ILogger<PaymentResponseConsumerWorker> logger)
        {
            _configuration = configuration;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
                GroupId = "payment-api-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using var consumer = new ConsumerBuilder<string, string>(config).Build();
            consumer.Subscribe("risk-evaluation-response");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(stoppingToken);
                    var response = JsonSerializer.Deserialize<RiskEvaluationResponse>(result.Message.Value);

                    if (response != null)
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var repo = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

                        var payment = await repo.GetByExternalIdAsync(response.ExternalOperationId, stoppingToken);
                        if (payment != null)
                        {
                            payment.UpdateStatus(response.Status == "accepted"
                                ? FinancialTransfers.Domain.Entities.PaymentStatus.Accepted
                                : FinancialTransfers.Domain.Entities.PaymentStatus.Denied);

                            await repo.UpdateAsync(payment, stoppingToken);
                            _logger.LogInformation("Pago {Id} actualizado a {Status}", payment.ExternalOperationId, payment.Status);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error procesando respuesta de riesgo");
                }
            }
        }
    }
}
