using DevopsApiSecurity;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>()
                  ?? throw new Exception("AppSettings not found");

builder.Services.AddSingleton(new AzureDevopsTokenValidator(appSettings.DevopsExtensionSecret));


builder.Build().Run();
