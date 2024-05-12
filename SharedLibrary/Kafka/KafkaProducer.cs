using Confluent.Kafka;
using Microsoft.Extensions.Configuration;

namespace SharedLibrary.Kafka;

public class KafkaProducer
{
    private readonly IProducer<Null, string> _producer;
    private readonly string _topic ;

    public KafkaProducer(string topic, IConfiguration configuration)
    {
        _topic = topic;
        var producerconfig = new ProducerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"]
        };

        _producer = new ProducerBuilder<Null, string>(producerconfig).Build();
    }

    public async Task ProduceAsync(string message)
    {
        var kafkamessage = new Message<Null, string> { Value = message, };

        await _producer.ProduceAsync(_topic, kafkamessage);
    }
}