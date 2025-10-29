using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;

namespace TripleAssignment;

public class AccessImages
{
    private readonly ILogger<AccessImages> _logger;

    public AccessImages(ILogger<AccessImages> logger)
    {
        _logger = logger;
    }

    [Function("AccessImages")]
    public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req,
            [BlobInput("images/image_{query.city}.jpg", Connection = "BLOB_STORAGE")] Stream blobItem
            )
    {
        _logger.LogInformation("City requested");

        HttpResponseData response = req.CreateResponse(System.Net.HttpStatusCode.OK);
        await blobItem.CopyToAsync(response.Body);

        return response;
    }
}
