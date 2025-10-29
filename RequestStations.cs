using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
    [QueueOutput("station-data", Connection = "AzureWebJobsStorage")]
    public async Task<IEnumerable<WeatherData>?> Run([QueueTrigger("start-process", Connection = "AzureWebJobsStorage")] string message)
    {
        _logger.LogInformation("C# Queue trigger function is processed", message);

        using HttpClient client = new HttpClient();
        string url = "https://data.buienradar.nl/2.0/feed/json";
        _logger.LogInformation("Sending the request");
        HttpResponseMessage response = await client.GetAsync(url);
        _logger.LogInformation("Got response");

        string responseString = await response.Content.ReadAsStringAsync();

        _logger.LogInformation("Parsing logs");
        JObject? responseData = JsonConvert.DeserializeObject<JObject>(responseString);
        JToken? stationList = responseData?["actual"]?["stationmeasurements"];
        IEnumerable<WeatherData>? tempList = stationList?.Select(x => new WeatherData
        {
            region = (string?)x["regio"],
            temperature = (float?)x["temperature"]
        }).ToList();

        IEnumerable<WeatherData>? devList = tempList?.Take(2);

        _logger.LogInformation("Returning list with station objects");

        return devList;
    }
}
