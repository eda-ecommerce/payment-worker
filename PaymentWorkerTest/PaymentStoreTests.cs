using eCommerceConsumerPlayground.Models;
using ECommerceConsumerPlayground.Models;
using ECommerceConsumerPlayground.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using paymentWorker.Models;

namespace PaymentWorkerTest;

public class PaymentStoreTests
{
    private readonly PaymentStore _sut;
    private readonly Mock<ILogger<PaymentStore>> _mockLogger = new Mock<ILogger<PaymentStore>>();

    private async Task<AppDbContext> GetDatabaseContext()
    {
        var optins = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var databaseContext = new AppDbContext(optins);
        databaseContext.Database.EnsureCreated();
        return databaseContext;
    }

    public PaymentStoreTests()
    {
        _sut = new PaymentStore(_mockLogger.Object, GetDatabaseContext().Result);
    }
    
    [Fact]
    public async Task SaveDataAsync_ShouldLogCorrectMessage_WhenDataIsStoredInTheDatabase()
    {
        // Arrage
        var payment1Id = Guid.NewGuid();
        var payment1OrderId = Guid.NewGuid();
        var payment1PaymentDate = DateOnly.FromDateTime(DateTime.Now);
        var payment1CreatedDate = DateOnly.FromDateTime(DateTime.Now);
        var payment1Status = Status.Unpayed;
        
        var payment1 = new Payment()
        {
            PaymentId = payment1Id,
            OrderId = payment1OrderId,
            PaymentDate = payment1PaymentDate,
            CreatedDate = payment1CreatedDate,
            Status = payment1Status
        };
        
        // Act
        _sut.SaveDataAsync(payment1);

        // Assert
        _mockLogger.Verify(logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.Is<EventId>(eventId => eventId.Id == 0),
                It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == $@"Payment with Id: '{payment1.PaymentId}' successfully saved in database.
OrderId: {payment1.OrderId}
CreatedDate: {payment1.CreatedDate}
Status: {payment1.Status}" 
                                                        && @type.Name == "FormattedLogValues"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
    [Fact]
    public async Task SaveDataAsync_ShouldLogCorrectMessage_WhenDataAlreadyExistsInDatabase()
    {
        // Arrage
        var payment1Id = Guid.NewGuid();
        var payment1OrderId = Guid.NewGuid();
        var payment1PaymentDate = DateOnly.FromDateTime(DateTime.Now);
        var payment1CreatedDate = DateOnly.FromDateTime(DateTime.Now);
        var payment1Status = Status.Unpayed;
        
        var payment1 = new Payment()
        {
            PaymentId = payment1Id,
            OrderId = payment1OrderId,
            PaymentDate = payment1PaymentDate,
            CreatedDate = payment1CreatedDate,
            Status = payment1Status
        };
        
        // Act
        _sut.SaveDataAsync(payment1);
        _sut.SaveDataAsync(payment1);

        // Assert
        _mockLogger.Verify(logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.Is<EventId>(eventId => eventId.Id == 0),
                It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == $@"Payment with Id: '{payment1.PaymentId}' already exists in database. No new persistence." 
                                                        && @type.Name == "FormattedLogValues"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}