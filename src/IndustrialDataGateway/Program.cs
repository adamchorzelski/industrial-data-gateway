using IndustrialDataGateway.Models;
using System.Text.Json;
using MQTTnet;
using Npgsql;
using Microsoft.Data.SqlClient;

var sqlBuilder = new SqlConnectionStringBuilder();
sqlBuilder.DataSource = "localhost";
sqlBuilder.InitialCatalog = "gateway_config";
sqlBuilder.IntegratedSecurity = true;
sqlBuilder.TrustServerCertificate = true;

await using var sqlConnection = new SqlConnection(sqlBuilder.ConnectionString);
await sqlConnection.OpenAsync();
Console.WriteLine("Connected to SQL Server");

string brokerHost;
int brokerPort;
string topic;

using (var command = new SqlCommand(
    "SELECT BrokerHost, BrokerPort, Topic FROM dbo.MqttSettings WHERE Id = 1",
    sqlConnection))
{
    await using var reader = await command.ExecuteReaderAsync();

    if (!await reader.ReadAsync())
    {
        throw new InvalidOperationException("MQTT configuration with Id = 1 not found");
    }

    brokerHost = reader.GetString(reader.GetOrdinal("BrokerHost"));
    brokerPort = reader.GetInt32(reader.GetOrdinal("BrokerPort"));
    topic = reader.GetString(reader.GetOrdinal("Topic"));
}

Console.WriteLine($"MQTT config: {brokerHost}:{brokerPort}, topic: {topic}");

var dbBuilder = new NpgsqlConnectionStringBuilder();
dbBuilder.Host = "127.0.0.1";
dbBuilder.Port = 5555;
dbBuilder.Database = "gateway_telemetry";
dbBuilder.Username = "gateway_user";
dbBuilder.Password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");

await using var dbConnection = new NpgsqlConnection(dbBuilder.ConnectionString);
await dbConnection.OpenAsync();
Console.WriteLine("Connected to PostgreSQL");

MqttClientFactory mqttFactory = new MqttClientFactory(); // tworzymy fabrykę: obiekt z metodami do tworzenia klienta i ustawień

var client = mqttFactory.CreateMqttClient(); // tworzemy klienta
client.ApplicationMessageReceivedAsync += HandleMqttMessage; //wskazujesz funkcję, którą MQTTnet ma uruchamiać po odebraniu wiadomości. Przy wywołaniu MQTTnet samo przekaże jej obiekt message
var optionsBuilder = mqttFactory.CreateClientOptionsBuilder(); // tworzymy opcje

optionsBuilder.WithTcpServer(brokerHost, brokerPort); // ustawiamy adres brokera: ten komputer, port 1883

var options = optionsBuilder.Build(); // pobieramy gotowe ustawienia połączenia i zapisujemy je w options

var subscribeBuilder = mqttFactory.CreateSubscribeOptionsBuilder(); // tworzymy opcje subscribera

subscribeBuilder.WithTopicFilter(topic); //  wybieramy temat, z którego chcemy ODBIERAĆ wiadomości

var subscribeOptions = subscribeBuilder.Build(); // pobieramy gotowe ustawienia subskrypcji

await client.ConnectAsync(options); // await variable.Method(argument); client łączy się z brokerem ; await czeka na zakończenie tej operacji

await client.SubscribeAsync(subscribeOptions); // client subskrybuje dany temat 

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
            string sql = "INSERT INTO telemetry_samples (device_id, value, unit, timestamp)\r\nVALUES (@device_id, @value, @unit, @timestamp);";
            using var command = new NpgsqlCommand(sql, dbConnection);
            command.Parameters.AddWithValue("device_id", receivedSample.DeviceId);
            command.Parameters.AddWithValue("value", receivedSample.Value);
            command.Parameters.AddWithValue("unit", receivedSample.Unit);
            command.Parameters.AddWithValue("timestamp", receivedSample.Timestamp);
            int rowsAffected = command.ExecuteNonQuery();
            Console.WriteLine($"Saved rows: {rowsAffected}");
        }
    }
    catch (JsonException ex)
    {
        Console.WriteLine($"Invalid message: {ex.Message}");
    }
    catch (NpgsqlException ex)
    {
        Console.WriteLine($"Database error: {ex.Message}");
    }
}

Task HandleMqttMessage(MqttApplicationMessageReceivedEventArgs message)
{
    string payload = message.ApplicationMessage.ConvertPayloadToString();
    ProcessMessage(payload);
    return Task.CompletedTask;
}