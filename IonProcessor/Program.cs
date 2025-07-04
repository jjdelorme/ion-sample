using Google.Cloud.BigQuery.V2;
using Google.Cloud.Logging.Console;
using Google.Cloud.Storage.V1;
using IonProcessor;
using IonProcessor.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddGoogleCloudConsole();

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<GoogleCloudOptions>(builder.Configuration.GetSection("GoogleCloud"));

builder.Services.AddSingleton(x => StorageClient.Create());
builder.Services.AddSingleton(x => BigQueryClient.Create(builder.Configuration["GoogleCloud:ProjectId"]));

builder.Services.AddScoped<IIonProcessingService, IonProcessingService>();
builder.Services.AddScoped<IBigQueryService, BigQueryService>();
builder.Services.AddScoped<IDecompressionService, NoOpDecompressionService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();

app.Run();