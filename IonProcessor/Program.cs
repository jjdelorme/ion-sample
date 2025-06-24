using Google.Cloud.BigQuery.V2;
using Google.Cloud.Storage.V1;
using IonProcessor;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<GoogleCloudOptions>(builder.Configuration.GetSection("GoogleCloud"));

builder.Services.AddSingleton(x => StorageClient.Create());
builder.Services.AddSingleton(x => BigQueryClient.Create(builder.Configuration["GoogleCloud:ProjectId"]));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();