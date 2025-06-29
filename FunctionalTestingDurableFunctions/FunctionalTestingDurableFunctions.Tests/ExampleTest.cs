namespace FunctionalTestingDurableFunctions.Tests;

using System.Text.Json;
using Flurl.Http;
using Shouldly;

public class ExampleTest : FixtureBase
{
    private const string FunctionUrl = $"http://localhost:8080/api/thing?code=custom-key";

    [Fact]
    public async Task ShouldWaitForExternalEventAsync()
    {
        var response = await FunctionUrl
            .AllowAnyHttpStatus()
            .PostJsonAsync(new ThingRequest());

        response.StatusCode.ShouldBe(202);

        var requestAcceptedResponse = await response.GetJsonAsync<RequestAcceptedResponse>();

        await WaitForFunctionToHaveStatusAsync(requestAcceptedResponse.StatusQueryGetUri, "Running");

        await RaiseEventAsync(requestAcceptedResponse.SendEventPostUri);

        await WaitForFunctionToHaveStatusAsync(requestAcceptedResponse.StatusQueryGetUri, "Completed");

        var finalStatusResponse = await requestAcceptedResponse.StatusQueryGetUri.GetJsonAsync<StatusResponse>();
        finalStatusResponse.Output.ShouldBe("The process was approved");
    }

    private async Task RaiseEventAsync(string sendEventUri)
    {
        var eventResponse = await sendEventUri
            .Replace("{eventName}", "SomeProcessApproval")
            .PostJsonAsync(new ExternalEvent());

        eventResponse.StatusCode.ShouldBe(202);
    }

    private async Task WaitForFunctionToHaveStatusAsync(string statusQueryGetUri, string status)
    {
        string lastReportedStatus = string.Empty;
        object lastReportedOutput = null;

        for (var i = 0; i < 10; i++)
        {
            var statusResponse = await statusQueryGetUri.GetJsonAsync<StatusResponse>();
            if (statusResponse.RuntimeStatus == status)
            {
                return;
            }

            lastReportedStatus = statusResponse.RuntimeStatus;
            lastReportedOutput = statusResponse.Output;

            await Task.Delay(1000);
        }

        Assert.Fail($"Function did not report running status in time. Last reported status was {lastReportedStatus}. Last reported status {JsonSerializer.Serialize(lastReportedOutput)}");
    }

    private record ThingRequest();
    private record RequestAcceptedResponse(string StatusQueryGetUri, string SendEventPostUri);
    private record StatusResponse(string RuntimeStatus, string Output);
}
