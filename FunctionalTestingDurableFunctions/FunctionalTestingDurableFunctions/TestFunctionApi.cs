namespace FunctionalTestingDurableFunctions;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;

public static class TestFunctionApi
{
    [Function(nameof(TestFunctionApi))]
    public static async Task<HttpResponseData> HttpStartAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "thing")] HttpRequestData req,
        [DurableClient] DurableTaskClient client,
        FunctionContext executionContext)
    {
        string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(TestFunctionOrchestrator));

        return await client.CreateCheckStatusResponseAsync(req, instanceId);
    }
}

public class TestFunctionOrchestrator
{
    [Function(nameof(TestFunctionOrchestrator))]
    public static async Task<string> RunOrchestratorAsync(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        await context.WaitForExternalEvent<ExternalEvent>("SomeProcessApproval");

        return "The process was approved";
    }
}

public class ExternalEvent();