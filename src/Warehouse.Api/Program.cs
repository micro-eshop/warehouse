using Warehouse.Infrastructure.Logging;
using MediatR;
using Warehouse.Core.QueryHandlers;
using Warehouse.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.UseLogging("Warehouse.Api");
await builder.AddInfrastructure();
builder.Services.AddControllers();
builder.Services.AddMediatR(typeof(GetProductStockQueryQueryHandler));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRequestLogging();

app.UseAuthorization();

app.MapControllers();

app.Run();
