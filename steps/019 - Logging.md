# Diving Into Logging

Structured logging vs. Non-Structured Logging

## Demo Time

Within `Home.razor`

Add `@Inject ILogger<Home> Logger`

Run the application and look at the difference!

## Practical Example - Cache Miss Logging

Maybe for diagnostics you want ot log cache misses.

Update `DashboardService` to inject `ILogger<DashboardService>` and replace the `cancel` operation with the following

```` csharp
async cancel =>
{
    logger.LogInformation("Dashboard cache miss for user {UserId}. Rebuilding summary.", userId);

    return await BuildSummaryAsync(
        userId.Value,
        cancel);
},
````

See how this can help!

## Serilog!

Lets get things ready for structured Logging outside of Aspire too!

### Install Packages

Add the following packages to the web project


```` powershell
Install-Package Serilog.AspnetCore
Install-Package Serilog.Sinks.Console
Install-Package Serilog.Sinks.OpenTelemetry
Install-Package Serilog.Settings.Configuration
````

### Configure `Serilog` in Program.cs

Just after the opening builder line, add the following

```` csharp
builder.Host.UseSerilog(
    (context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(
                context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console();
    });
````

Update appsettings.json and replace the existing "Logging" section with the below.  This will write logs to Console, File & OpenTelemetry!

```` xml
    "Serilog": {
        "MinimumLevel": {
            "Default": "Information",
            "Override": {
                "Microsoft": "Warning",
                "Microsoft.EntityFrameworkCore.Database.Command": "Information"
            }
        },
        "WriteTo": [
            {
                "Name": "Console"
            },
            {
                "Name": "File",
                "Args": {
                    "path": "log.log",
                    "rollingInterval": "Day"
                }
            },
            {
                "Name": "OpenTelemetry"
            }
        ]
    },
````
>[!NOTE]
>We now have control over limits

# Discussion Points

* Why Serilog
* Scopes & Extra Stuff

## Scopes

If we wanted to, we can create a log-Scope which will auto-add properties to individual log entries, so we don't have to repeat ourselves.  Below is an example

```` csharp
using var logScope =
    logger.BeginScope(
        new Dictionary<string, object?>
        {
            ["UserId"] = userId,
            ["ProjectId"] = request.ProjectId
        });
logger.LogInformation("Testing");
````

In this example the "Testing" message will ALSO have the UserId and ProjectId values, even though we didn't add it!