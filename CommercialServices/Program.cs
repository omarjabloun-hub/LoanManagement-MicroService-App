using CommercialServices;
using SharedLibrary;
using SharedLibrary.Kafka;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Configuration
    .AddEnvironmentVariables()
    .Build();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var loanApplicationConsumer = new CommercialKafkaConsumer("LoanApplicationQueue", "CommercialService", app.Configuration);
loanApplicationConsumer.ExecuteAsync(new CancellationToken());

app.Run();
