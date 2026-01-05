using System;
using System.Text.Json.Serialization;

namespace apppasteleriav04.Models.Domain
{
    public class DeliveryPerson
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("userid")]
        public Guid UserId { get; set; }

        [JsonPropertyName("full_name")]
        public string FullName { get; set; } = string.Empty;

        [JsonPropertyName("phone")]
        public string Phone { get; set; } = string.Empty;

        [JsonPropertyName("is_available")]
        public bool IsAvailable { get; set; }

        [JsonPropertyName("current_latitude")]
        public double? CurrentLatitude { get; set; }

        [JsonPropertyName("current_longitude")]
        public double? CurrentLongitude { get; set; }

        [JsonPropertyName("vehicle_type")]
        public string VehicleType { get; set; } = string.Empty;

        [JsonPropertyName("license_plate")]
        public string LicensePlate { get; set; } = string.Empty;

        [JsonPropertyName("active_orders_count")]
        public int ActiveOrdersCount { get; set; }
    }
}
