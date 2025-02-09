namespace FunctionalTestingExample;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

public class DoubleMyNumberFunction(ILogger<DoubleMyNumberFunction> logger)
{
    [Function(nameof(DoubleMyNumberFunction))]
    public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "double-my-number")] HttpRequest req)
    {
        logger.LogInformation("Received a request to double a number...");

        var request = await req.ReadFromJsonAsync<DoubleMyNumberRequest>();
        if (request == null)
        {
            return new BadRequestObjectResult("A request object must be supplied");
        }

        return new OkObjectResult(request.Number * 2);
    }
}

public record DoubleMyNumberRequest(double Number);
