namespace FunctionalTestingDurableFunctions.Tests;

using System.Diagnostics;
using System.Net;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using Testcontainers.Azurite;

public class FixtureBase : IAsyncLifetime
{
    protected AzuriteContainer? AzuriteContainer;
    private IContainer? container;
    private INetwork? network;
    protected string? AzuriteConnectionString;

    public async Task InitializeAsync()
    {
        await BuildApplicationContainerAsync();
        await CreateNetworkAsync();
        await StartAzuriteAsync();
        await StartFunctionAppContainerAsync();
    }

    private async Task CreateNetworkAsync()
    {
        network = new NetworkBuilder()
            .WithName(Guid.NewGuid().ToString("N"))
            .Build();

        await network.CreateAsync();
    }

    private async Task StartAzuriteAsync()
    {
        AzuriteContainer = new AzuriteBuilder()
            .WithImage("mcr.microsoft.com/azure-storage/azurite:3.33.0")
            .WithNetwork(network)
            .WithNetworkAliases("azurite")
            .WithCommand("--skipApiVersionCheck")
            .Build();

        await AzuriteContainer.StartAsync();

        var connectionProperties = AzuriteContainer.GetConnectionString()
            .Split(";")
            .Select(part => part.Split('=', 2))
            .ToDictionary(pair => pair[0], pair => pair[1]);

        connectionProperties["BlobEndpoint"] = $"http://azurite:10000/devstoreaccount1";
        connectionProperties["QueueEndpoint"] = $"http://azurite:10001/devstoreaccount1";
        connectionProperties["TableEndpoint"] = $"http://azurite:10002/devstoreaccount1";

        AzuriteConnectionString = string.Join(";", connectionProperties.Select(x => $"{x.Key}={x.Value}"));
    }

    private async Task StartFunctionAppContainerAsync()
    {
        var hostJsonLocation = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "test-host", "host.json"));

        container = new ContainerBuilder()
            .WithImage("app-under-test")
            .WithName("app-under-test")
            .WithEnvironment("Storage", AzuriteConnectionString)
            .WithEnvironment("WEBSITE_HOSTNAME", "localhost:8080")
            .WithEnvironment("YourEnvVariable1", "something")
            .WithEnvironment("AzureWebJobsSecretStorageType", "files")
            .WithResourceMapping(hostJsonLocation, "/azure-functions-host/Secrets")
            .WithNetwork(network)
            .WithPortBinding(8080, 80)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilHttpRequestIsSucceeded(req => req.ForPath("/")
                    .ForStatusCode(HttpStatusCode.OK)))
            .Build();

        await container.StartAsync();
    }

    private async Task BuildApplicationContainerAsync()
    {
        var appDirectory = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "..", "FunctionalTestingDurableFunctions"));

        var startInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = "build -t app-under-test .",
            WorkingDirectory = appDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process();
        process.StartInfo = startInfo;
        process.Start();

        await process.WaitForExitAsync();

        Console.WriteLine($"Exited with code {process.ExitCode}");
    }

    public async Task DisposeAsync()
    {
        if (container is not null)
        {
            await container.DisposeAsync();
        }

        if (AzuriteContainer is not null)
        {
            await AzuriteContainer.DisposeAsync();
        }

        if (network is not null)
        {
            await network.DisposeAsync();
        }
    }
}