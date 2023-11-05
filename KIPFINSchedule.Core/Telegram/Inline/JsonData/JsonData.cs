using Newtonsoft.Json;

namespace KIPFINSchedule.Core.Telegram.Inline.JsonData;

public class JsonData
{
    [JsonProperty("I")]
    public int? Int { get; set; }
    [JsonProperty("S")]
    public string? String { get; set; }
    [JsonProperty("B")]
    public bool? Bool { get; set; }
    [JsonProperty(nameof(Step))]
    public Step? Step { get; set; }
}

public enum Step
{
    Start,
    SelectCourse,
    SelectGroup,
    StepBack,
    Exit
}