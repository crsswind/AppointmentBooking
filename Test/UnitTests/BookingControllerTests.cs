using Api.Controllers;
using Api.DTOs;
using Api.Models;
using Application.Contracts;
using Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Globalization;

namespace Test.UnitTests
{
    public class BookingControllerTests
    {
        [Fact]
        public async Task QueryAvailableSlots_Returns_Ok_When_Request_Is_Valid()
        {
            // Arrange
            var mockSlotService = new Mock<ISlotService>();
            var mockLogger = new Mock<ILogger<BookingController>>();
            var controller = new BookingController(mockSlotService.Object, mockLogger.Object);
            var request = new QueryRequest
            {
                Date = new DateTime(2024, 5, 3),
                Products = ["SolarPanels", "Heatpumps"],
                Language = "German",
                Rating = "Gold"
            };

            var availableSlots = new List<AvailableSlotDTO>
            {
                new AvailableSlotDTO { StartDate = DateTime.ParseExact("2024-05-03T10:30:00.000Z", "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal), AvailableCount = 1 },
                new AvailableSlotDTO { StartDate = DateTime.ParseExact("2024-05-03T11:00:00.000Z", "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal), AvailableCount = 1 },
                new AvailableSlotDTO { StartDate = DateTime.ParseExact("2024-05-03T11:30:00.000Z", "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal), AvailableCount = 1 }
            };

            mockSlotService
                .Setup(service => service.GetAvailableSlotsAsync(It.IsAny<DateTime>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(availableSlots);

            // Act
            var result = await controller.QueryAvailableSlotsAsync(request, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedSlots = Assert.IsType<List<AvailableSlotResponse>>(okResult.Value);

            Assert.Equal(3, returnedSlots.Count);
            Assert.Equal("2024-05-03T10:30:00.000Z", returnedSlots[0].StartDate);
            Assert.Equal(1, returnedSlots[0].AvailableCount);
        }

        [Fact]
        public async Task QueryAvailableSlots_Returns_BadRequest_When_Products_Are_Missing()
        {
            // Arrange
            var mockSlotService = new Mock<ISlotService>();
            var mockLogger = new Mock<ILogger<BookingController>>();
            var controller = new BookingController(mockSlotService.Object, mockLogger.Object);
            var request = new QueryRequest { Date = new DateTime(2024, 5, 3), Rating = "Silver", Language = "English" };

            // Act
            var result = await controller.QueryAvailableSlotsAsync(request, CancellationToken.None);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task QueryAvailableSlots_Returns_InternalServerError_WhenExceptionThrown()
        {
            // Arrange
            var mockSlotService = new Mock<ISlotService>();
            var mockLogger = new Mock<ILogger<BookingController>>();
            var controller = new BookingController(mockSlotService.Object, mockLogger.Object);
            mockSlotService
                .Setup(service => service.GetAvailableSlotsAsync(It.IsAny<DateTime>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Some error"));

            var request = new QueryRequest
            {
                Date = new DateTime(2024, 5, 3),
                Products = ["SolarPanels", "Heatpumps"],
                Language = "German",
                Rating = "Gold"
            };

            // Act
            var result = await controller.QueryAvailableSlotsAsync(request, CancellationToken.None);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
            var problemDetails = Assert.IsType<ProblemDetails>(badRequestResult.Value);

            Assert.Equal(500, badRequestResult.StatusCode);
            Assert.Equal("An unexpected error occurred while processing your request.", problemDetails.Detail);
        }
    }
}