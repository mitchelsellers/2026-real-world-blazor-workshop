namespace ProjectHub.Application.TimeTravel;

[RegisterScoped]
internal class TimeTravel : ITimeTravel
{
    public DateTime DT { get; set; } = DateTime.Now;
}
