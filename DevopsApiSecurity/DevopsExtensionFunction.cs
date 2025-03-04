namespace DevopsApiSecurity;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

public class DevopsExtensionFunction(AzureDevopsTokenValidator azureDevopsTokenValidator, ILogger<DevopsExtensionFunction> logger)
{
    [Function(nameof(DevopsExtensionFunction))]
    public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        logger.LogInformation("Processing repository summary request");

        return await ExecuteIfCalledFromDevopsExtensionAsync(req, async () =>
        {
            var request = await req.ReadFromJsonAsync<RequestFromDevops>();
            if (request == null)
            {
                logger.LogError("Request body was not supplied or could not be deserialized");

                return new BadRequestObjectResult("Request body must be supplied");
            }

            //do your thing, return your thing

            return new OkResult();
        }, async () =>
        {
            logger.LogWarning("Unauthorized access attempt");
            return await Task.FromResult(new UnauthorizedResult());
        });
    }

    private async Task<IActionResult> ExecuteIfCalledFromDevopsExtensionAsync(HttpRequestData req, Func<Task<IActionResult>> action, Func<Task<IActionResult>> unauthorized)
    {
        if (!req.Headers.TryGetValues("Authorization", out var authHeaders))
        {
            logger.LogError("No Authorization header found in request");
            return await unauthorized();
        }

        var token = authHeaders.FirstOrDefault()?.Replace("Bearer ", "");
        if (string.IsNullOrEmpty(token))
        {
            logger.LogError("No bearer token found in Authorization header");
            return await unauthorized();
        }

        var principal = azureDevopsTokenValidator.Validate(token);
        if (principal == null)
        {
            logger.LogError("Token validation failed");
            return await unauthorized();
        }

        logger.LogInformation("Token validation successful");
        return await action();
    }
}

public record RequestFromDevops();