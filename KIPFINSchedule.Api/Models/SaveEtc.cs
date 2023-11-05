using Newtonsoft.Json;

namespace KIPFINSchedule.Api.Models;

public class SaveEtc
{   
    [JsonProperty("itemFormat")]
    public required string ItemFormat { get; set; }
    [JsonProperty("useMondayTime")]
    public bool UseMondayTime { get; set; }
    [JsonProperty("mondayTime")]
    public required string MondayTime { get; set; }
    [JsonProperty("otherDays")]
    public List<string> OtherDays { get; set; } = null!;
}