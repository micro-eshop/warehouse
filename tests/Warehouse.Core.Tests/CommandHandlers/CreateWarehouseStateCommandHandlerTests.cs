using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

using LanguageExt;

using Moq;
using Warehouse.Core.CommandHandlers;
using Warehouse.Core.Commands;
using Warehouse.Core.Model;

using Xunit;

using static LanguageExt.Prelude;
using Warehouse.Core.Repositories;

using Unit = MediatR.Unit;

namespace Warehouse.Core.Tests.CommandHandlers;

public class CreateWarehouseStateCommandHandlersTests
{
    [Fact]
    public async Task TestCreateWarehouseStateCommandWhenEverythingIsOk()
    {
        // Arrange
        var repo = new Mock<IWarehouseWriter>();
        repo.Setup(x => x.Write(It.IsAny<IReadOnlyCollection<Stock>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<Either<Exception, LanguageExt.Unit>>(Right(LanguageExt.Unit.Default)));
        var productId = new ProductId(1);
        var command = new CreateWarehouseStateCommand(productId);
        var handler = new CreateWarehouseStateCommandHandler(repo.Object);

        // Act
        var subject = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(Unit.Value, subject);

        repo.Verify(x => x.Write(It.IsAny<IReadOnlyCollection<Stock>>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}