using Confluent.Kafka;
using FinancialTransfers.Application.Interfaces;
using System.Text.Json;

namespace FinancialTransfers.RiskEngine;

public class RiskEvaluationWorker : BackgroundService
{
    private readonly ILogger<RiskEvaluationWorker> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;

    public RiskEvaluationWorker(
        ILogger<RiskEvaluationWorker> logger,
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            GroupId = "risk-evaluation-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe("risk-evaluation-request");

        _logger.LogInformation("--> Risk Evaluation Worker iniciado y escuchando...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = consumer.Consume(TimeSpan.FromMilliseconds(500));

                if (consumeResult == null) continue;

                var messageJson = consumeResult.Message.Value;
                var request = JsonSerializer.Deserialize<RiskEvaluationRequest>(messageJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (request != null)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var riskService = scope.ServiceProvider.GetRequiredService<IRiskService>();

                    await riskService.ProcessRiskEvaluationAsync(request, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando mensaje de Kafka");
                await Task.Delay(2000, stoppingToken);
            }
        }

        consumer.Close();
    }
}