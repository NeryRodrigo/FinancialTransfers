using Confluent.Kafka;
using FinancialTransfers.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FinancialTransfers.Infraestructure.Messaging
{
    public class KafkaProducer : IKafkaProducer
    {
        private readonly IProducer<string, string> _producer;

        public KafkaProducer(IConfiguration config)
        {
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = config["Kafka:BootstrapServers"] ?? "localhost:9092"
            };
            _producer = new ProducerBuilder<string, string>(producerConfig).Build();
        }

        public async Task PublishRiskRequestAsync(object message, CancellationToken ct)
        {
            var json = JsonSerializer.Serialize(message);
            await _producer.ProduceAsync("risk-evaluation-request", new Message<string, string> { Value = json }, ct);
        }

        public async Task PublishRiskResponseAsync(RiskEvaluationResponse response, CancellationToken ct)
        {
            var json = JsonSerializer.Serialize(response);
            await _producer.ProduceAsync("risk-evaluation-response", new Message<string, string> { Value = json }, ct);
        }
    }
}
