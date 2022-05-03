using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using Warehouse.Core.Commands;
using Warehouse.Core.Model;

namespace Warehouse.Infrastructure.Seed;

public static class DataSeed
{
    public async static Task InitDb(this IApplicationBuilder builder)
    {
        using var scope = builder.ApplicationServices.CreateScope();
        var sender = scope.ServiceProvider.GetService<ISender>();
        foreach (var id in Enumerable.Range(1, 20))
        {
            await sender.Send(new CreateWarehouseStateCommand(new ProductId(id)));
        }
    }
}