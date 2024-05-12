using System.Text.Json;
using Confluent.Kafka;
using SharedLibrary;
using SharedLibrary.Kafka;

namespace CommercialServices;

public class CommercialKafkaConsumer : IKafkaConsumer
{
    private readonly IConsumer<Ignore, string> _consumer;
    
    private readonly string _topic;
    private readonly IConfiguration _configuration;

    public CommercialKafkaConsumer(string topic, string groupId, IConfiguration configuration)
    {
        _topic = topic;
        _configuration = configuration;
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
            var documents = JsonSerializer.Deserialize<List<Document>>(message);
            if(documents == null)
            {
                Console.WriteLine("Error deserializing documents");
            }
            else
            {
                Console.WriteLine("Sending Documents to OCR Service ...");
                var ocrProducer = new KafkaProducer("OcrQueue", _configuration);
                ocrProducer.ProduceAsync(documents.ToString());
                Console.WriteLine("Documents sent to OCR service for processing.");
            }
            var loanApplication = JsonSerializer.Deserialize<LoanApplicationDto>(message);
            if(loanApplication == null)
            {
                Console.WriteLine("Error deserializing loan application");
            }
            else
            {
                Console.WriteLine("Save loan application to Loan database ...");
                // Save loan application to Loan database
                Console.WriteLine("Loan application saved to Loan database.");
                Console.WriteLine("Checking if loan application is initially eligible...");
                if(loanApplication.Income > 100000)
                {
                    Console.WriteLine("Loan application is initially eligible.");
                    var EligibilityProducer = new KafkaProducer("EligibilityQueue", _configuration);
                    EligibilityProducer.ProduceAsync(loanApplication.ToString());
                }
                else
                {
                    Console.WriteLine("Loan application is not initially eligible.");
                    var NotificationProducer = new KafkaProducer("NotificationQueue", _configuration);
                    NotificationProducer.ProduceAsync("Loan application is not initially eligible.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error processing Kafka message");
        }
    }
}