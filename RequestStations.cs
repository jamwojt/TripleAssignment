using Microsoft.Azure.Functions.Worker;
using Azure.Storage.Queues;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.Functions.Worker.Http;
using static System.Convert;
using System.Text;

namespace TripleAssignment;

public class WeatherData
{
    public string? region { get; set; }
    public float? temperature { get; set; }
}

public class RequestStations
{
    private readonly ILogger<RequestStations> _logger;

    public RequestStations(ILogger<RequestStations> logger)
    {
        _logger = logger;
    }

    [Function(nameof(RequestStations))]
    public async Task Run([QueueTrigger("start-process", Connection = "AzureWebJobsStorage")] string message)
    {
        try
        {

            _logger.LogInformation("Function is processed with message: {message}", message);

            string? connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

            if (connectionString == null)
            {
                _logger.LogError("Environment variable START_QUEUE does not exist");
            }

            QueueClient queueClient = new QueueClient(connectionString, "station-data", new QueueClientOptions{
                MessageEncoding = QueueMessageEncoding.Base64
            });

            using HttpClient client = new HttpClient();
            string url = "https://data.buienradar.nl/2.0/feed/json";
            _logger.LogInformation("Sending the request");
            HttpResponseMessage response = await client.GetAsync(url);
            _logger.LogInformation("Got response");

            string responseString = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Parsing response");
            JObject? responseData = JsonConvert.DeserializeObject<JObject>(responseString);
            JToken? measurements = responseData?["actual"]?["stationmeasurements"];
            IEnumerable<WeatherData>? stationList = measurements?.Select(x => new WeatherData
            {
                region = (string?)x["regio"],
                temperature = (float?)x["temperature"]
            }).ToList();

            _logger.LogInformation("Writing station data to queue");
            foreach (WeatherData item in stationList?.Take(2) ?? [])
            {
                string serializedItem = JsonConvert.SerializeObject(item);
                byte[] byteMsg = Encoding.UTF8.GetBytes(serializedItem);
                string encodedMsg = Convert.ToBase64String(byteMsg);
                await queueClient.SendMessageAsync(serializedItem);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex.ToString());
            throw ex;
        }
    }
}
