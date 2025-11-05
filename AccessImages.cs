using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;
using Azure.Storage.Blobs;
using System.Text;

namespace TripleAssignment;

public class AccessImages
{
    private readonly ILogger<AccessImages> _logger;

    public AccessImages(ILogger<AccessImages> logger)
    {
        _logger = logger;
    }

    [Function("AccessImages")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        _logger.LogInformation("City requested");

        string? connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

        if (connectionString == null)
        {
            _logger.LogError("Can't find connection string from environment variable");
        }

        string? city = req.Query.Get("city");
        if (city == null)
        {
            _logger.LogWarning("No city was provided in the request.");
            HttpResponseData badRequestResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            badRequestResponse.Headers.Add("Content-Type", "Application/json");
            string errMsg = "{\"error\": \"Use the city query parameter (?city=)\"}";
            await badRequestResponse.Body.WriteAsync(Encoding.UTF8.GetBytes(errMsg));
            return badRequestResponse;
        }

        _logger.LogInformation("Got city from request: {city}", city);

        string fileName = $"image_{city}.jpg";
        _logger.LogInformation("file name: {name}", fileName);
        try
        {

            BlobClient containerClient = new BlobClient(connectionString, "images", fileName);
            Azure.Storage.Blobs.Models.BlobDownloadResult image = await containerClient.DownloadContentAsync();
            BinaryData result = image.Content;
            HttpResponseData response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "image/jpg");
            await response.Body.WriteAsync(result);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            HttpResponseData badRequestResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            badRequestResponse.Headers.Add("Content-Type", "Application/json");
            string errMsg = "{\"error\": \"This city does not have an image stored. Please chose another city.\"}";
            await badRequestResponse.Body.WriteAsync(Encoding.UTF8.GetBytes(errMsg));
            return badRequestResponse;
        }
    }
}
