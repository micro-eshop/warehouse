using FastEndpoints;

using Warehouse.Api.Request;
using Warehouse.Api.Response;
using MediatR;
using Warehouse.Core.Queries;
using Warehouse.Core.Model;
using LanguageExt.UnsafeValueAccess;

namespace Warehouse.Api.Endpoints;

public class GetProductStockEndpoint : Endpoint<GetProductStockRequest, GetProductStockResponse>
{
    private readonly ISender _sender;
    private readonly ILogger<GetProductStockEndpoint> _logger;

    public GetProductStockEndpoint(ISender sender, ILogger<GetProductStockEndpoint> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    public override void Configure()
    {
        AllowAnonymous();
        Verbs(Http.GET);
        Routes("/warehouse/{WarehouseId}/products/{ProductId}/stock");
    }

    public override async Task HandleAsync(GetProductStockRequest req, CancellationToken ct)
    {
        var result = await _sender.Send(new GetProductStockQuery(new ProductId(req.ProductId), new WarehouseId(req.WarehouseId)), ct);
        if (result.IsNone)
        {
            _logger.LogWarning("Warehouse state not found, ProductId={ProductId} ShopNumber={WarehouseId}", req.ProductId, req.WarehouseId);
            await SendNotFoundAsync();
            return;
        }
        var stock = result.ValueUnsafe();
        var response = new GetProductStockResponse()
        {
            AvailableQuantity = stock.Quantity,
            WarehouseId = req.WarehouseId,
            ProductId = req.ProductId
        };

        await SendAsync(response);
    }
}