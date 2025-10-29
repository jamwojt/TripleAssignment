using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TripleAssignment;

public class ProcessWeatherData
{
    private readonly ILogger<ProcessWeatherData> _logger;

    public ProcessWeatherData(ILogger<ProcessWeatherData> logger)
    {
        _logger = logger;
    }

    [Function(nameof(ProcessWeatherData))]
    [BlobOutput("images/image_{region}.jpg", Connection = "AzureWebJobsStorage")]
    public async Task<byte[]?> Run([QueueTrigger("station-data", Connection = "AzureWebJobsStorage")] string message)
    {
        _logger.LogInformation("Got message", message);

        HttpClient client = new HttpClient();
        string url = "https://images.unsplash.com/photo-1461988320302-91bde64fc8e4?ixid=2yJhcHBfaWQiOjEyMDd9&fm=jpg&fit=crop&w=1080&q=80&fit=max";

        _logger.LogInformation("Sending request with image url");
        byte[] imageResponse = await client.GetByteArrayAsync(url);

        JObject? msg = JsonConvert.DeserializeObject<JObject>(message);

        string formatMsg = $"{msg?["region"]}: {msg?["temperature"]} degrees";
        _logger.LogInformation($"formatted message: {formatMsg}");
        _logger.LogInformation("Writing message on the image");
        byte[]? drawnImage = ImageDrawer.AddTextToImage(imageResponse, formatMsg);

        if (drawnImage == null)
        {
            // if data is not there, don't upload anything
            _logger.LogInformation("Drawing failed. returning normal image");
            return null;
        }

        _logger.LogInformation("Drawing succeeded. Returning image");

        return drawnImage;
    }
}
