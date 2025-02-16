using System.Text.Json.Serialization;

namespace Api.DTOs
{
    public class AvailableSlotResponse
    {
        [JsonPropertyName("start_date")]
        public string StartDate { get; set; }

        [JsonPropertyName("available_count")]
        public int AvailableCount { get; set; }
    }
}