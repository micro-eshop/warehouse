using System.Threading;
using System.Threading.Tasks;

using Moq;
using LanguageExt;
using Warehouse.Core.Model;
using Warehouse.Core.Queries;
using Warehouse.Core.QueryHandlers;
using Warehouse.Core.Repositories;
using static LanguageExt.Prelude;
using FluentAssertions;
using Xunit;
using LanguageExt.UnsafeValueAccess;
using System;

namespace Warehouse.Core.Tests.QueryHandlers;

public class GetProductStockQueryQueryHandlerTests
{
    [Fact]
    public async Task GetProductStockWhenNotExists_ShouldReturnNone()
    {
        // Arrange

        var mockRepo = new Mock<IWarehouseReader>();
        mockRepo.Setup(x => x.GetStock(It.IsAny<ProductId>(), It.IsAny<WarehouseId>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<Option<Stock>>(None));
        var query = new GetProductStockQuery(new ProductId(1), new WarehouseId(1));

        var handler = new GetProductStockQueryQueryHandler(mockRepo.Object);

        // Act

        var subject = await handler.Handle(query, CancellationToken.None);

        // Assert

        Assert.Equal(None, subject);

        mockRepo.Verify(x => x.GetStock(It.IsAny<ProductId>(), It.IsAny<WarehouseId>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetProductStockWhenExists_ShouldReturnSomeStock()
    {
        // Arrange
        var productId = new ProductId(1);
        var warehouseId = new WarehouseId(1);
        var stock = new Stock(productId, warehouseId, new StockQuantity(10, 5));
        var mockRepo = new Mock<IWarehouseReader>();
        mockRepo.Setup(x => x.GetStock(It.IsAny<ProductId>(), It.IsAny<WarehouseId>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Some(stock)));
        var query = new GetProductStockQuery(productId, warehouseId);

        var handler = new GetProductStockQueryQueryHandler(mockRepo.Object);

        // Act

        var subject = await handler.Handle(query, CancellationToken.None);
        // Assert
        subject.IsSome.Should().BeTrue();
        subject.ValueUnsafe().Should().Be(new AvailableQuantity(5));

        mockRepo.Verify(x => x.GetStock(It.IsAny<ProductId>(), It.IsAny<WarehouseId>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetProductStockWhenRepoThrowsException()
    {
        // Arrange
        var productId = new ProductId(1);
        var warehouseId = new WarehouseId(1);
        var stock = new Stock(productId, warehouseId, new StockQuantity(10, 5));
        var mockRepo = new Mock<IWarehouseReader>();
        mockRepo.Setup(x => x.GetStock(It.IsAny<ProductId>(), It.IsAny<WarehouseId>(), It.IsAny<CancellationToken>()))
            .Throws(new Exception("Test exception"));
        var query = new GetProductStockQuery(productId, warehouseId);

        var handler = new GetProductStockQueryQueryHandler(mockRepo.Object);

        // Act and test
        Assert.ThrowsAnyAsync<Exception>(() => handler.Handle(query, CancellationToken.None));

        mockRepo.Verify(x => x.GetStock(It.IsAny<ProductId>(), It.IsAny<WarehouseId>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}