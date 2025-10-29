using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;

namespace TripleAssignment;

public class StartProcess
{
    private readonly ILogger<StartProcess> _logger;

    public StartProcess(ILogger<StartProcess> logger)
    {
        _logger = logger;
    }

    [Function("StartProcess")]
    [QueueOutput("start-process", Connection = "START_QUEUE")]
    public HttpResponseData Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req,
            out string msgQueue
            )
    {
        _logger.LogInformation("Putting start process message on queue");

        msgQueue = "start";

        HttpResponseData response = req.CreateResponse(System.Net.HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "Application/json");
        response.WriteString("{process: \"started\"}");

        return response;
    }
}
