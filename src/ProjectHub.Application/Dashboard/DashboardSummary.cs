namespace ProjectHub.Application.Dashboard;

public sealed record DashboardSummary(
    int ProjectCount,
    int OpenWorkItems,
    int AssignedToMe,
    int OverdueWorkItems,
    int CompletedThisWeek)
{
    public static DashboardSummary Empty { get; } = new DashboardSummary(0, 0, 0, 0, 0);
}