using Newtonsoft.Json;

namespace KIPFINSchedule.Core.Telegram.Inline.JsonData;

public class InlineJson
{
    [JsonProperty("C")]
    public required string Command { get; set; }
    [JsonProperty("JD")]
    public required JsonData JsonData { get; set; }
}