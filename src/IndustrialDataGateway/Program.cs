using IndustrialDataGateway.Models;
using System.Text.Json;

TelemetrySample firstSample = new TelemetrySample(); // tworzymy jeden obiekt klasy 

firstSample.DeviceId = "sensor-01";
firstSample.Value = 3.7;
firstSample.Unit = "A";

Console.WriteLine($"{firstSample.DeviceId}: {firstSample.Value} {firstSample.Unit}");

string json = JsonSerializer.Serialize(firstSample);
Console.WriteLine(json);

TelemetrySample secondSample = new TelemetrySample(); // tworzymy jeden obiekt klasy 

secondSample.DeviceId = "sensor-02";
secondSample.Value = 24;
secondSample.Unit = "V";

Console.WriteLine($"{secondSample.DeviceId}: {secondSample.Value} {secondSample.Unit}");

secondSample.Value = 23.45;
secondSample.Unit = "°C";

Console.WriteLine($"{secondSample.DeviceId}: {secondSample.Value} {secondSample.Unit}");

firstSample.Value = 12.5;

Console.WriteLine($"1 obiket: {firstSample.Value}, 2 obiket: {secondSample.Value}");


TelemetrySample sample = JsonSerializer.Deserialize<TelemetrySample>(json);
Console.WriteLine($"to jest DESERIALIZACJA: {sample.DeviceId}: {sample.Value} {sample.Unit}");
//deserializacja zawsze tworzy nowy obiekt, nie „przywraca” starego

//test
string text = """{"DeviceId":"sensor-03","value":55.1,"Unit":"bar"}"""; // specjalnie value a nie Value
TelemetrySample test = JsonSerializer.Deserialize<TelemetrySample>(text);
Console.WriteLine($"to jest DESERIALIZACJA: {test.DeviceId}: {test.Value} {test.Unit}");



