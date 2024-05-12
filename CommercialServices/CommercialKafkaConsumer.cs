using System.Text.Json;
using Confluent.Kafka;
using SharedLibrary;
using SharedLibrary.DbContext;
using SharedLibrary.Kafka;

namespace CommercialServices;

public class CommercialKafkaConsumer : IKafkaConsumer
{
    private readonly IConsumer<Ignore, string> _consumer;
    
    private readonly string _topic;
    private readonly IConfiguration _configuration;
    private readonly LoanDbContext _context;

    public CommercialKafkaConsumer(IConfiguration configuration, LoanDbContext context)
    {
        _topic = "LoanApplicationQueue";
        _configuration = configuration;
        _context = context;
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            GroupId = "CommercialService",
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

            try{
                var loanApplication = System.Text.Json.JsonSerializer.Deserialize<LoanApplicationFullDto>(message);
                Console.WriteLine("Sending Documents to OCR Service ...");
                var ocrProducer = new KafkaProducer("OcrQueue", _configuration);
                var serializedDocuments = JsonSerializer.Serialize(loanApplication.Documents);
                ocrProducer.ProduceAsync(serializedDocuments);
                Console.WriteLine("Documents sent to OCR service for processing.");
                try
                {
                    Console.WriteLine("Saving documents to Document database ...");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }catch(Exception ex){
                Console.WriteLine("Error deserializing documents  {0}", ex);
            }

            try
            {
                var loanApplication = JsonSerializer.Deserialize<LoanApplicationFullDto>(message);
                var loanToDb = new LoanApplication(loanApplication.Id, loanApplication.Income, loanApplication.FullName, loanApplication.HasDebt);
                // Save loan application to Loan database
                try
                {
                    Console.WriteLine("Saving loan application to Loan database ...");
                    _context.Set<LoanApplication>().Add(loanToDb);
                }catch(Exception ex){
                    Console.WriteLine("Error saving loan application to Loan database  {0}", ex);
                }
                Console.WriteLine("Loan application saved to Loan database.");
                Console.WriteLine("Checking if loan application is initially eligible...");
                if(loanApplication.Income > 100000)
                {
                    Console.WriteLine("Loan application is initially eligible.");
                    var eligibilityProducer = new KafkaProducer("EligibilityQueue", _configuration);
                    var serializedLoanApplication = JsonSerializer.Serialize(loanApplication);
                    eligibilityProducer.ProduceAsync(serializedLoanApplication);
                    Console.WriteLine("Loan application sent to Risk Management service for further processing.");
                }
                else
                {
                    Console.WriteLine("Loan application is not initially eligible.");
                    var notificationProducer = new KafkaProducer("NotificationQueue", _configuration);
                    notificationProducer.ProduceAsync("Loan application is not initially eligible.");
                    Console.WriteLine("Client Notified of loan application status(Not Eligible).");
                }
            }catch(Exception ex){
                Console.WriteLine("Error deserializing loan application  {0}", ex);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error processing Kafka message {0}", ex);
        }
    }
}