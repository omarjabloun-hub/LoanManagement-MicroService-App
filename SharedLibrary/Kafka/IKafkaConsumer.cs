namespace SharedLibrary.Kafka;

public interface IKafkaConsumer
{
    public Task ExecuteAsync(CancellationToken stoppingToken);
}