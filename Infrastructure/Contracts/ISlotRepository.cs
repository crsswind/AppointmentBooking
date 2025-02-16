using Infrastructure.Models;

namespace Infrastructure.Contracts
{
    public interface ISlotRepository
    {
        Task<IEnumerable<AvailableSlotDbModel>> GetAvailableSlotsAsync(DateTime date, string[] products, string language, string rating, CancellationToken cancellationToken);
    }
}