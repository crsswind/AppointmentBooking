using Application.DTOs;

namespace Application.Contracts
{
    public interface ISlotService
    {
        Task<IEnumerable<AvailableSlotDTO>> GetAvailableSlotsAsync(DateTime date, string[] products, string language, string rating, CancellationToken cancellationToken);
    }
}