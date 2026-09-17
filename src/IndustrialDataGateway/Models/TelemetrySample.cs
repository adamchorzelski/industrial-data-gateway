namespace IndustrialDataGateway.Models;

public class TelemetrySample
{
    public string DeviceId { get; set; } = "";
    public double Value { get; set; }
    public string Unit { get; set; } = "";
}