# Creating Custom Activity Sources

ProjectHubDiagnostics.cs

```` csharp
using System.Diagnostics;

namespace ProjectHub.Application.Diagnostics;

internal static class ProjectHubDiagnostics
{
    public const string ActivitySourceName =
        "ProjectHub";

    public static readonly ActivitySource ActivitySource =
        new(ActivitySourceName);
}
````

Add this to the OpenTelemetry Configuration

```` csharp
tracing.AddSource("ProjectHub");
````

Consider magic names here as well

## Using In Code

From our dashboard service we can implement with a private member

```` csharp
private static readonly ActivitySource ActivitySource =
    new("ProjectHub");
````

and then updated method

```` csharp
private async Task<DashboardSummary> BuildSummaryAsync(
    Guid userId,
    CancellationToken cancellationToken)
{
    using var activity =
        ActivitySource.StartActivity(
            "BuildDashboardSummary");

    activity?.SetTag(
        "projecthub.user.id",
        userId);

    // existing work...
}
````

# Discussion Points

* What does this get us?
* What makes a good tag?