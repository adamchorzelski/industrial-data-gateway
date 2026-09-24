using System.Text.Json.Serialization;

namespace IndustrialDataGateway.Models;

public class TelemetrySample
{
    [JsonRequired]
    public string DeviceId { get; set; } = "";
    [JsonRequired]
    public double Value { get; set; }
    [JsonRequired]
    public string Unit { get; set; } = "";
    [JsonRequired]
    public DateTimeOffset Timestamp { get; set; }

    public string Describe()
    {
        return $"{DeviceId}: {Value} {Unit} {Timestamp}";
    }
}