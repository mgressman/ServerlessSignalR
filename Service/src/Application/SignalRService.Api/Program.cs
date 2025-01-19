// create an app registration that represents the signalr service in azure ad. the authenticationauthority and
// authenticationclientid will come from this app registration. any client that wants to connect to the signalr
// service will need to authenticate with this app registration.
//
// when deploying to Azure:
//  - make sure publish target of application is linux-x64 if running on a linux app service
//  - add the following values to the environment variables when deploying to Azure:
//      - AzureSignalRConnectionString:
//      - AuthenticationAuthority: https://login.microsoftonline.com/{tenant-id}
//      - AuthenticationClientId
using Microsoft.ApplicationInsights;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Exceptions;
using Serilog.Extensions.Logging;
using Serilog.Formatting.Display;
using SignalRService.ApplicationCore.SignalRHubs;
using SignalRService.Infrastructure.Middleware.Authentication;

var builder = FunctionsApplication.CreateBuilder(args);

// if you want this function app to behave as a webapi and work with
// the underlying HTTP request and response objects using types from
// asp.net core, including httprequest, httpresponse, and iactionresult,
// add the call to configurefunctionswebapplication.
builder.ConfigureFunctionsWebApplication();

builder.Configuration.AddUserSecrets<Program>();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights()
    .AddServerlessHub<DefaultHub>()

    .AddLogging(loggingBuilder =>
    {
        // TODO: console logs appear to be duplicated. clearproviders is not working as expected. it seems the default functions console log continues to log to the console on top of our serilogger.
        loggingBuilder.ClearProviders();
        loggingBuilder.AddSerilog();
    })

    .AddSingleton<ILoggerProvider>(serviceProvider =>
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .WriteTo.ApplicationInsights(
                serviceProvider.GetRequiredService<TelemetryClient>(),
                TelemetryConverter.Traces)
            .WriteTo.Console(
                new MessageTemplateTextFormatter(
                    "{NewLine}[{Timestamp:yyyy/MM/dd HH:mm:ss} {Level:u11}] {Message:lj} : {Exception}"))
            .CreateLogger();
        return new SerilogLoggerProvider(Log.Logger, true);
    })

    .AddMediatR(cfg =>
        cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));

builder.Logging.Services.Configure<LoggerFilterOptions>(static options =>
{
    // the application insights sdk adds a default logging filter that instructs
    // ilogger to capture only warning and more severe logs. application insights
    // requires an explicit override. log levels can also be configured using appsettings.json.
    var defaultRule = options.Rules.FirstOrDefault(rule =>
        rule.ProviderName == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");

    if (defaultRule is not null)
    {
        options.Rules.Remove(defaultRule);
    }
});

//builder.UseMiddleware<AuthenticationMiddleware>();

builder.Build().Run();
