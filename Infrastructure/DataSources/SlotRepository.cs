using Dapper;
using Infrastructure.Contracts;
using Infrastructure.Models;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Infrastructure.DataSources
{
    public class SlotRepository(string connectionString, ILogger<SlotRepository> logger) : ISlotRepository
    {
        private async Task<NpgsqlConnection> GetOpenConnectionAsync()
        {
            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            return connection;
        }

        private const string GetAvailableSlotsQuery = @"
            SELECT s.start_date AS StartDate,
                   COUNT(DISTINCT s.sales_manager_id) AS AvailableCount
            FROM slots s
            JOIN sales_managers sm ON s.sales_manager_id = sm.id
            LEFT JOIN slots bs ON s.sales_manager_id = bs.sales_manager_id AND bs.booked AND s.start_date < bs.end_date AND s.end_date > bs.start_date
            WHERE s.start_date::date = @Date
              AND NOT s.booked
              AND sm.languages @> @Language::varchar[]
              AND sm.products @> @Products::varchar[]
              AND sm.customer_ratings @> @Rating::varchar[]
              AND bs.id IS NULL
            GROUP BY s.start_date
            ORDER BY s.start_date;";

        public async Task<IEnumerable<AvailableSlotDbModel>> GetAvailableSlotsAsync(DateTime date, string[] products, string language, string rating, CancellationToken cancellationToken)
        {
            try
            {
                using var connection = await GetOpenConnectionAsync();

                var parameters = new
                {
                    date.Date,
                    Products = products,
                    Language = new[] { language },
                    Rating = new[] { rating }
                };

                var command = new CommandDefinition(GetAvailableSlotsQuery, parameters, cancellationToken: cancellationToken);
                return await connection.QueryAsync<AvailableSlotDbModel>(command);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching available slots.");
                throw;
            }
        }
    }
}