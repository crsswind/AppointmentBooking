using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using Api.DTOs;
using Api.Models;
using Application.Contracts;

namespace Api.Controllers;

[ApiController]
[Route("calendar")]
public class BookingController : ControllerBase
{
    private readonly ISlotService _slotService;
    private readonly ILogger<BookingController> _logger;

    public BookingController(ISlotService slotService, ILogger<BookingController> logger)
    {
        _slotService = slotService;
        _logger = logger;
    }

    [HttpPost("query")]
    public async Task<ActionResult<IEnumerable<AvailableSlotResponse>>> QueryAvailableSlotsAsync([FromBody] QueryRequest request, CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        if (request.Products == null || !request.Products.Any())
        {
            errors.Add("At least one product must be specified.");
        }

        if (string.IsNullOrEmpty(request.Rating))
        {
            errors.Add("Rating is not specified.");
        }

        if (string.IsNullOrEmpty(request.Language))
        {
            errors.Add("Language is not specified.");
        }

        if (errors.Any())
        {
            return BadRequest(new ProblemDetails
            {
                Type = "Invalid Request",
                Title = "Invalid request parameters.",
                Status = StatusCodes.Status400BadRequest,
                Detail = "The request contains missing or invalid parameters.",
                Extensions = { ["errors"] = errors }
            });
        }

        try
        {
            var availableSlots = await _slotService.GetAvailableSlotsAsync(request.Date, request.Products, request.Language, request.Rating, cancellationToken);

            var response = availableSlots.Select(slot => new AvailableSlotResponse
            {
                StartDate = slot.StartDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture),
                AvailableCount = slot.AvailableCount
            }).ToList();

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while getting available slots.");

            var problemDetails = new ProblemDetails
            {
                Type = "Internal Server Error",
                Title = "Internal Server Error",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "An unexpected error occurred while processing your request."
            };

            if (HttpContext?.TraceIdentifier != null)
            {
                problemDetails.Extensions["traceId"] = HttpContext.TraceIdentifier;
            }

            return StatusCode(500, problemDetails);
        }
    }
}
