using CommercialServices;
using Microsoft.EntityFrameworkCore;
using SharedLibrary;
using SharedLibrary.DbContext;
using SharedLibrary.Kafka;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Configuration
    .AddEnvironmentVariables()
    .Build();
builder.Services.AddDbContext<LoanDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration["POSTGRES_CONNECTION_STRING"]);
});
builder.Services.AddScoped<IKafkaConsumer, CommercialKafkaConsumer>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var loanApplicationConsumer = app.Services.GetRequiredService<IKafkaConsumer>();
loanApplicationConsumer.ExecuteAsync(new CancellationToken());

app.Run();
