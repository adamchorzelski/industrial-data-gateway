using IndustrialDataGateway.Models;
using System.Text.Json;

TelemetrySample firstSample = new TelemetrySample(); // tworzymy jeden obiekt klasy 

firstSample.DeviceId = "sensor-01";
firstSample.Value = 3.7;
firstSample.Unit = "A";
firstSample.Timestamp = DateTimeOffset.UtcNow;

Console.WriteLine(firstSample.Describe());

string json = JsonSerializer.Serialize(firstSample);
Console.WriteLine(json);

TelemetrySample secondSample = new TelemetrySample(); // tworzymy jeden obiekt klasy 

secondSample.DeviceId = "sensor-02";
secondSample.Value = 24;
secondSample.Unit = "V";
secondSample.Timestamp = DateTimeOffset.UtcNow;

Console.WriteLine(secondSample.Describe());

secondSample.Value = 23.45;
secondSample.Unit = "°C";

Console.WriteLine(secondSample.Describe());

firstSample.Value = 12.5;

Console.WriteLine($"1 obiket: {firstSample.Value}, 2 obiket: {secondSample.Value}");


TelemetrySample sample = JsonSerializer.Deserialize<TelemetrySample>(json);
Console.WriteLine($"to jest DESERIALIZACJA: {sample.DeviceId}: {sample.Value} {sample.Unit}");
//deserializacja zawsze tworzy nowy obiekt, nie „przywraca” starego

//test
string text = """{"DeviceId":"sensor-03","value":55.1,"Unit":"bar"}"""; // specjalnie value a nie Value
string text2 = """{"DeviceId":"sensor-03","Value":55.1,"Unit":"bar", "Timestamp":"2026-09-19T11:00:00+00:00"} huuh}""";
try
{
    TelemetrySample test = JsonSerializer.Deserialize<TelemetrySample>(text);
    Console.WriteLine($"to jest DESERIALIZACJA: {test.DeviceId}: {test.Value} {test.Unit}");
}
catch (JsonException ex)
{
    Console.WriteLine($"Invalid message: {ex.Message}");
}
try
{
    TelemetrySample test2 = JsonSerializer.Deserialize<TelemetrySample>(text2);
    Console.WriteLine($"to jest DESERIALIZACJA: {test2.DeviceId}: {test2.Value} {test2.Unit}");
}
catch (JsonException ex)
{
    Console.WriteLine($"Invalid message: {ex.Message}");
}
try
{
    TelemetrySample? emptySample = JsonSerializer.Deserialize<TelemetrySample>("""{"DeviceId":"sensor-03","Value":0,"Unit":"bar", "Timestamp":"2026-09-19T11:00:00+00:00"}""");
    //czy istnieje obiekt;
    if (emptySample is null)
    {
        Console.WriteLine("No sample received");
    }
    //czy DeviceId ma treść
    else if (string.IsNullOrWhiteSpace(emptySample.DeviceId)) //Zwraca true, gdy tekst jest null, pusty albo zawiera wyłącznie białe znaki
    {
        Console.WriteLine("brakuje DeviceId to identyfikacji urzadzenia!");
    }
    //czy Unit ma treść
    else if (string.IsNullOrWhiteSpace(emptySample.Unit))
    {
        Console.WriteLine("brakuje Unit żeby zdefiniować jednostkę pomiaru");
    }
    else
    {
        Console.WriteLine($"to jest DESERIALIZACJA: {emptySample.DeviceId}: {emptySample.Value} {emptySample.Unit}");
    }
}
catch (JsonException ex)
{
    Console.WriteLine($"Invalid message: {ex.Message}");
}

TelemetrySample? nullSample = JsonSerializer.Deserialize<TelemetrySample>(json);
if (nullSample is null)
{
    Console.WriteLine("No sample received");
}
else
{
    Console.WriteLine("--------------------");
    Console.WriteLine(nullSample.Describe());
    Console.WriteLine("--------------------");
}

Console.WriteLine("++++++++++++++++++++++++++++");
ProcessMessage(json);
text = """{"DeviceId":"sensor-03","Value":55.1,"Unit":"bar", "Timestamp":"2026-09-19T11:00:00+00:00"}""";
ProcessMessage(text);
text = """{"DeviceId":"","Value":55.1,"Unit":"bar", "Timestamp":"2026-09-19T11:00:00+00:00"}""";
ProcessMessage(text);
text = "null";
ProcessMessage(text);
text = """{"DeviceId":"sensor-03","Value":55.1,"Unit":"", "Timestamp":"2026-09-19T11:00:00+00:00"}""";
ProcessMessage(text);

ProcessMessage(text2);

void ProcessMessage(string payload)
{
    try
    {
    TelemetrySample? deserializeString = JsonSerializer.Deserialize<TelemetrySample>(payload);
        //czy istnieje obiekt;
        if (deserializeString is null)
        {
            Console.WriteLine("No sample received");
        }
        //czy DeviceId ma treść
        else if (string.IsNullOrWhiteSpace(deserializeString.DeviceId)) //Zwraca true, gdy tekst jest null, pusty albo zawiera wyłącznie białe znaki
        {
            Console.WriteLine("The DeviceId required to identify the device is missing!");
        }
        //czy Unit ma treść
        else if (string.IsNullOrWhiteSpace(deserializeString.Unit))
        {
            Console.WriteLine("The Unit is missing to define the unit of measurement");
        }
        else
        {
            Console.WriteLine($"Deserialize {deserializeString.DeviceId}: {deserializeString.Value} {deserializeString.Unit} {deserializeString.Timestamp}");
        }
    }
    catch (JsonException ex)
    {
        Console.WriteLine($"Invalid message: {ex.Message}");
    }
}