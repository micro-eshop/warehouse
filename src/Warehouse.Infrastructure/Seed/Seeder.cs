using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Warehouse.Core.Commands;
using Warehouse.Core.Model;

namespace Warehouse.Infrastructure.Seed;

public static class DataSeed
{
    public async static Task InitDb(this WebApplication builder)
    {
        builder.Logger.LogInformation("Start Data Migration");
        using var scope = builder.Services.CreateScope();
        var sender = scope.ServiceProvider.GetService<ISender>();
        foreach (var id in Enumerable.Range(1, 20))
        {
            await sender!.Send(new CreateWarehouseStateCommand(new ProductId(id)));
        }
        builder.Logger.LogInformation("End Data Migration");
    }
}