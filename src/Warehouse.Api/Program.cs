using Warehouse.Infrastructure.Logging;
using MediatR;
using Warehouse.Core.QueryHandlers;
using Warehouse.Infrastructure.Extensions;
using FastEndpoints;
using FastEndpoints.Swagger;
using Warehouse.Infrastructure.Seed;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.UseLogging("Warehouse.Api");
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

app.UseRequestLogging();

app.UseAuthorization();
app.UseFastEndpoints();

app.MapControllers();

app.Run();
