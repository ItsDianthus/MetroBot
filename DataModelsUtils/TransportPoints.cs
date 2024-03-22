using System.Text.Json.Serialization;
using CsvHelper.Configuration.Attributes;

namespace DataModelsUtils
{
    /// <summary>
    /// Represents a transport point entity with various properties.
    /// </summary>
    public class TransportPoints
    {
        [JsonPropertyName("ID")]
        [Name("ID")]
        public int Id { get; set; }

        [JsonPropertyName("TPUName")]
        [Name("TPUName")]
        public string? TPUName { get; set; }

        [JsonPropertyName("global_id")]
        [Name("global_id")]
        public long GlobalId { get; set; }

        [JsonPropertyName("AdmArea")]
        [Name("AdmArea")]
        public string? AdmArea { get; set; }

        [JsonPropertyName("District")]
        [Name("District")]
        public string? District { get; set; }

        [JsonPropertyName("NearStation")]
        [Name("NearStation")]
        public string? NearStation { get; set; }

        [JsonPropertyName("YearOfComissioning")]
        [Name("YearOfComissioning")]
        public int YearOfComissioning { get; set; }

        [JsonPropertyName("Status")]
        [Name("Status")]
        public string? Status { get; set; }

        [JsonPropertyName("AvailableTransfer")]
        [Name("AvailableTransfer")]
        public string? AvailableTransfer { get; set; }

        [JsonPropertyName("CarCapacity")]
        [Name("CarCapacity")]
        public double? CarCapacity { get; set; }

        [JsonPropertyName("geodata_center")]
        [Name("geodata_center")]
        public double? GeodataCenter { get; set; }

        [JsonPropertyName("geoarea")]
        [Name("geoarea")]
        public double? Geoarea { get; set; }
    }
}