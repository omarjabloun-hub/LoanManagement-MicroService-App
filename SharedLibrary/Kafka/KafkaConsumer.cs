using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SharedLibrary.Kafka;

public class KafkaConsumer : IKafkaConsumer
{
    private readonly IConsumer<Ignore, string> _consumer;
    
    private readonly string _topic;

    public KafkaConsumer(string topic, string groupId, IConfiguration configuration)
    {
        _topic = topic;
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
    }

    public Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(_topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            ProcessKafkaMessage(stoppingToken);
            
            Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }

        _consumer.Close();
        return Task.CompletedTask;
    }

    private void ProcessKafkaMessage(CancellationToken stoppingToken)
    {
        try
        {
            var consumeResult = _consumer.Consume(stoppingToken);

            var message = consumeResult.Message.Value;
            Console.WriteLine($"message received: {message}");
            
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error processing Kafka message");
        }
    }
}