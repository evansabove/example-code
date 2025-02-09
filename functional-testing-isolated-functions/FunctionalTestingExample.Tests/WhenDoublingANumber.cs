namespace FunctionalTestingExample.Tests;

using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using UnitTestEx;
using UnitTestEx.Azure.Functions;
using UnitTestEx.Xunit;
using Xunit.Abstractions;

public class WhenDoublingANumber(ITestOutputHelper helper) : UnitTestBase(helper)
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(-2, -4)]
    [InlineData(3.5, 7)]
    public async Task ShouldDoubleTheNumberAsync(double number, int expectedResult)
    {
        using var functionTester = FunctionTester.Create<Program>();

        var triggerTester = functionTester
            .HttpTrigger<DoubleMyNumberFunction>()
            .WithRouteCheck(RouteCheckOption.None);

        var requestBody = new DoubleMyNumberRequest(number);

        var request = functionTester.CreateHttpRequest(HttpMethod.Post, "api/double-my-number", JsonSerializer.Serialize(requestBody), "application/json");
        var response = await triggerTester.RunAsync(f => f.RunAsync(request));

        response.Result.ShouldBeOfType<OkObjectResult>();

        var responseBody = ((OkObjectResult)response.Result).Value as double?;
        responseBody.ShouldNotBeNull();

        responseBody.Value.ShouldBe(expectedResult);
    }
}
