namespace Warehouse.Core.Tests.Model;

using Xunit;
using FluentAssertions;
using Warehouse.Core.Model;

public class StockModelTests
{
    [Theory]
    [InlineData(1, 1, 1, 1, 0)]
    [InlineData(1, 1, 1, 2, 0)]
    [InlineData(1, 1, 2, 1, 1)]
    [InlineData(1, 1, 200, 100, 100)]
    public void TestGetAvailableQuantity(int productid, int warehouseid, int stockq, int reservedq, int expectedAvailableQuantity)
    {
        // Arrange
        var stock = new Stock(new ProductId(productid), new WarehouseId(warehouseid), new StockQuantity(stockq, reservedq));

        // Act
        var availableQuantity = stock.GetAvailableQuantity();

        // Assert

        availableQuantity.Should().Be(expectedAvailableQuantity);
    }
}