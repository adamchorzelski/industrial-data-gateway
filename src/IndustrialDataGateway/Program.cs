using IndustrialDataGateway.Models;
using System.Text.Json;
using MQTTnet;

MqttClientFactory mqttFactory = new MqttClientFactory(); // tworzymy fabrykę: obiekt z metodami do tworzenia klienta i ustawień

var client = mqttFactory.CreateMqttClient(); // tworzemy klienta
client.ApplicationMessageReceivedAsync += HandleMqttMessage; //wskazujesz funkcję, którą MQTTnet ma uruchamiać po odebraniu wiadomości. Przy wywołaniu MQTTnet samo przekaże jej obiekt message
var optionsBuilder = mqttFactory.CreateClientOptionsBuilder(); // tworzymy opcje

optionsBuilder.WithTcpServer("localhost", 1883); // ustawiamy adres brokera: ten komputer, port 1883

var options = optionsBuilder.Build(); // pobieramy gotowe ustawienia połączenia i zapisujemy je w options

var subscribeBuilder = mqttFactory.CreateSubscribeOptionsBuilder(); // tworzymy opcje subscribera

subscribeBuilder.WithTopicFilter("factory/temperature"); //  wybieramy temat, z którego chcemy ODBIERAĆ wiadomości

var subscribeOptions = subscribeBuilder.Build(); // pobieramy gotowe ustawienia subskrypcji

await client.ConnectAsync(options); // await variable.Method(argument); client łączy się z brokerem ; await czeka na zakończenie tej operacji

await client.SubscribeAsync(subscribeOptions); // client subskrybuje dany temat 





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

ProcessMessage(json);
//deserializacja zawsze tworzy nowy obiekt, nie „przywraca” starego

//test
string text = """{"DeviceId":"sensor-03","value":55.1,"Unit":"bar"}"""; // specjalnie value a nie Value
string text2 = """{"DeviceId":"sensor-03","Value":55.1,"Unit":"bar", "Timestamp":"2026-09-19T11:00:00+00:00"} huuh}""";

ProcessMessage(text);

ProcessMessage(text2);

ProcessMessage("""{"DeviceId":"sensor-03","Value":0,"Unit":"bar", "Timestamp":"2026-09-19T11:00:00+00:00"}""");

ProcessMessage(json);

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

Console.ReadLine();

await client.DisconnectAsync();

void ProcessMessage(string payload)
{
    try
    {
        TelemetrySample? receivedSample = JsonSerializer.Deserialize<TelemetrySample>(payload);
        //czy istnieje obiekt;
        if (receivedSample is null)
        {
            Console.WriteLine("No sample received");
        }
        //czy DeviceId ma treść
        else if (string.IsNullOrWhiteSpace(receivedSample.DeviceId)) //Zwraca true, gdy tekst jest null, pusty albo zawiera wyłącznie białe znaki
        {
            Console.WriteLine("The DeviceId required to identify the device is missing!");
        }
        //czy Unit ma treść
        else if (string.IsNullOrWhiteSpace(receivedSample.Unit))
        {
            Console.WriteLine("The Unit is missing to define the unit of measurement");
        }
        else
        {
            Console.WriteLine($"Deserialize {receivedSample.DeviceId}: {receivedSample.Value} {receivedSample.Unit} {receivedSample.Timestamp}");
        }
    }
    catch (JsonException ex)
    {
        Console.WriteLine($"Invalid message: {ex.Message}");
    }
}

Task HandleMqttMessage(MqttApplicationMessageReceivedEventArgs message)
{
    string payload = message.ApplicationMessage.ConvertPayloadToString();
    ProcessMessage(payload);
    return Task.CompletedTask;
}