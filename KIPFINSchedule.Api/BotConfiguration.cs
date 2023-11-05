namespace KIPFINSchedule.Api;

public class BotConfiguration
{
    public required string BotToken { get; init; }
    public required string HostAddress { get; init; }
    public required string Route { get; init; }
    public required string SecretToken { get; init; }
}