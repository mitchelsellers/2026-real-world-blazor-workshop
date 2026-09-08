namespace ProjectHub.Application.Dashboard;

public sealed record DashboardSummary(
    int ProjectCount,
    int OpenWorkItems,
    int AssignedToMe,
    int OverdueWorkItems,
    int CompletedThisWeek);