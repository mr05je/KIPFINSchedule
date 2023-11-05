namespace KIPFINSchedule.Api.Models;

public class GetEtc
{
    public required string ItemFormat { get; set; }
    public bool UseMondayTime { get; set; }
    public required string MondayTime { get; set; }
    public List<string> OtherDays { get; set; } = null!;
}