using Application.Services;
using Infrastructure.Contracts;
using Infrastructure.Models;
using Microsoft.Extensions.Logging;
using Moq;
using System.Globalization;

namespace Test.UnitTests
{
    public class SlotServiceTests
    {
        [Fact]
        public async Task GetAvailableSlots_Returns_AvailableSlots_When_Input_Is_Valid()
        {
            // Arrange
            var mockSlotRepository = new Mock<ISlotRepository>();
            var mockLogger = new Mock<ILogger<SlotService>>();
            var slotService = new SlotService(mockSlotRepository.Object, mockLogger.Object);
            var date = new DateTime(2024, 5, 3);
            var products = new[] { "SolarPanels", "Heatpumps" };
            var language = "German";
            var rating = "Gold";
            var cancellationToken = CancellationToken.None;

            var dbSlots = new List<AvailableSlotDbModel>
            {
                new AvailableSlotDbModel { StartDate = new DateTime(2024, 5, 3, 10, 30, 0), AvailableCount = 1 },
                new AvailableSlotDbModel { StartDate = new DateTime(2024, 5, 3, 11, 0, 0), AvailableCount = 1 },
                new AvailableSlotDbModel { StartDate = new DateTime(2024, 5, 3, 11, 30, 0), AvailableCount = 1 }
            };

            mockSlotRepository
                .Setup(repo => repo.GetAvailableSlotsAsync(It.IsAny<DateTime>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dbSlots);

            // Act
            var result = await slotService.GetAvailableSlotsAsync(date, products, language, rating, cancellationToken);

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Equal(3, resultList.Count);
            Assert.Equal(DateTime.ParseExact("2024-05-03T10:30:00.000Z", "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal), resultList[0].StartDate);
            Assert.Equal(1, resultList[0].AvailableCount);
            Assert.Equal(DateTime.ParseExact("2024-05-03T11:00:00.000Z", "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal), resultList[1].StartDate);
            Assert.Equal(1, resultList[1].AvailableCount);
            Assert.Equal(DateTime.ParseExact("2024-05-03T11:30:00.000Z", "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal), resultList[2].StartDate);
            Assert.Equal(1, resultList[2].AvailableCount);
        }

        [Fact]
        public async Task GetAvailableSlots_ReturnsEmptyList_WhenNoSlotsAvailable()
        {
            // Arrange
            var mockSlotRepository = new Mock<ISlotRepository>();
            var mockLogger = new Mock<ILogger<SlotService>>();
            var slotService = new SlotService(mockSlotRepository.Object, mockLogger.Object);
            var date = new DateTime(2024, 5, 3);
            var products = new[] { "SolarPanels" };
            var language = "Persian";
            var rating = "Silver";
            var cancellationToken = CancellationToken.None;

            mockSlotRepository
                .Setup(repo => repo.GetAvailableSlotsAsync(It.IsAny<DateTime>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AvailableSlotDbModel>());

            // Act
            var result = await slotService.GetAvailableSlotsAsync(date, products, language, rating, cancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAvailableSlots_LogsErrorAndThrowsException_WhenRepositoryThrowsException()
        {
            // Arrange
            var mockSlotRepository = new Mock<ISlotRepository>();
            var mockLogger = new Mock<ILogger<SlotService>>();
            var slotService = new SlotService(mockSlotRepository.Object, mockLogger.Object);
            var date = new DateTime(2024, 5, 3);
            var products = new[] { "SolarPanels" };
            var language = "German";
            var rating = "Gold";
            var cancellationToken = CancellationToken.None;

            mockSlotRepository
                .Setup(repo => repo.GetAvailableSlotsAsync(date, products, language, rating, cancellationToken))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                slotService.GetAvailableSlotsAsync(date, products, language, rating, cancellationToken));

            Assert.Equal("Database error", exception.Message);

            mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to retrieve available slots.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}