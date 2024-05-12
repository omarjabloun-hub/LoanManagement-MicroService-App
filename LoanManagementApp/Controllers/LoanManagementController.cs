using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SharedLibrary;
using SharedLibrary.Kafka;

namespace LoanManagementApp.Controllers;

[ApiController]
[Route("[controller]")]
public class LoanManagementController : ControllerBase
{
    private readonly KafkaProducer _kafkaProducer;
    public LoanManagementController(IConfiguration configuration)
    {
        _kafkaProducer = new KafkaProducer("LoanApplicationQueue" , configuration);
    }

    [HttpPost("loan-application")]
    public async Task<IActionResult> SubmitLoanApplication([FromForm] LoanApplicationDto loanApplicationDto, [FromForm] IFormFile[] documents)
    {
        if (documents == null || documents.Length == 0)
        {
            return BadRequest("No documents provided.");
        }

        var random = new Random();
        var documentTasks = documents.Select(async d => {
            using var memoryStream = new MemoryStream();
            await d.CopyToAsync(memoryStream);
            return new Document(
                random.Next(100),
                d.FileName,
                d.ContentType,
                d.Length,
                memoryStream.ToArray()
            );
        });

        var documentsArray = await Task.WhenAll(documentTasks);

        var loanApplication = new LoanApplicationFullDto(
            Guid.NewGuid(),
            loanApplicationDto.Income,
            loanApplicationDto.FullName,
            loanApplicationDto.HasDebt,
            documentsArray.ToList()
        );
        var serializedLoanApplication = System.Text.Json.JsonSerializer.Serialize(loanApplication);
        Console.WriteLine($"Sending : {serializedLoanApplication}");
        try
        {
            await _kafkaProducer.ProduceAsync(serializedLoanApplication);
            Console.WriteLine("Loan application sent to Loan application Queue");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send loan application: {ex.Message}");
            return Problem("Failed to send loan application to Kafka.");
        }

        return Ok("Loan application sent to Loan application Queue.");
    }
}