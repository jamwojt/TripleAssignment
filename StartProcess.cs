using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;
using Azure.Storage.Queues;
using static System.Convert;
using System.Text;

namespace TripleAssignment;

public class StartProcess
{
    private readonly ILogger<StartProcess> _logger;

    public StartProcess(ILogger<StartProcess> logger)
    {
        _logger = logger;
    }

    [Function("StartProcess")]
    public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req
            )
    {
        _logger.LogInformation("Putting start process message on queue");

        string? connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

        if (connectionString == null)
        {
            _logger.LogError("Environment variable START_QUEUE does not exist");
            HttpResponseData errResponse = req.CreateResponse(System.Net.HttpStatusCode.FailedDependency);
            errResponse.Headers.Add("Content-Type", "Application/json");
            await errResponse.WriteStringAsync("{\"error\": \"Cannot connect to the queue.\"}");
            return errResponse;
        }

        QueueClient queueClient = new QueueClient(connectionString, "start-process", new QueueClientOptions
        {
            MessageEncoding = QueueMessageEncoding.Base64
        });
        byte[] byteMsg = Encoding.UTF8.GetBytes("start");
        string encodedMsg = ToBase64String(byteMsg);
        await queueClient.SendMessageAsync(encodedMsg);


        HttpResponseData response = req.CreateResponse(System.Net.HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "Application/json");
        await response.WriteStringAsync("{\"process\": \"started\"}");

        return response;
    }
}
