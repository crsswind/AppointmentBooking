using Application.Contracts;
using Application.DTOs;
using Infrastructure.Contracts;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class SlotService : ISlotService
    {
        private readonly ISlotRepository _slotRepository;
        private readonly ILogger<SlotService> _logger;

        public SlotService(ISlotRepository slotRepository, ILogger<SlotService> logger)
        {
            _slotRepository = slotRepository;
            _logger = logger;
        }
        public async Task<IEnumerable<AvailableSlotDTO>> GetAvailableSlotsAsync(DateTime date, string[] products, string language, string rating, CancellationToken cancellationToken)
        {
            try
            {
                var slots = await _slotRepository.GetAvailableSlotsAsync(date, products, language, rating, cancellationToken);

                var result = slots.Select(slot => new AvailableSlotDTO
                {
                    StartDate = slot.StartDate,
                    AvailableCount = slot.AvailableCount
                });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve available slots.");
                throw;
            }
        }
    }
}