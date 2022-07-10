using Warehouse.Infrastructure.Logging;
using MediatR;
using Warehouse.Core.QueryHandlers;
using Warehouse.Infrastructure.Extensions;
using FastEndpoints;
using FastEndpoints.Swagger;

using OpenTelemetry.Trace;

using Warehouse.Infrastructure.Seed;
using Warehouse.Infrastructure.Telemetry;

var telemetry = new OpenTelemetryConfiguration() { ServiceName = "warehouse", ServiceVersion = "1.0.0" };
var builder = WebApplication.CreateBuilder(args);

builder.AddOpenTelemetryLogging(telemetry);
builder.AddOpenTelemetryTracing(telemetry, b =>
{
    b.AddRedisInstrumentation();
    b.AddNatsSource();
});
builder.AddOpenTelemetryMetrics(telemetry);
// Add services to the container.
await builder.AddInfrastructure();
builder.Services.AddControllers();
builder.Services.AddMediatR(typeof(GetProductStockQueryQueryHandler));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDoc();
builder.Services.AddFastEndpoints();
var app = builder.Build();

await DataSeed.InitDb(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi3(s => s.ConfigureDefaults());
}

app.UseHttpLogging();

app.UseAuthorization();
app.UseFastEndpoints();

app.MapControllers();

app.Run();
