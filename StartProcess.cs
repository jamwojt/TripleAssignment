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
    [QueueOutput("start-process", Connection = "AzureWebJobsStorage")]
    public string Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
    {
        _logger.LogInformation("Putting start process message on queue");

        return "start";
    }
}
