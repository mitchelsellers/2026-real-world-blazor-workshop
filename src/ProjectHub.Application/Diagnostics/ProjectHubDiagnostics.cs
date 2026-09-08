using System.Diagnostics;

namespace ProjectHub.Application.Diagnostics;

internal static class ProjectHubDiagnostics
{
    public const string ActivitySourceName =
        "ProjectHub";

    public static readonly ActivitySource ActivitySource =
        new(ActivitySourceName);
}