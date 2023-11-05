using Newtonsoft.Json;

namespace KIPFINSchedule.Api.Models;

public class SaveFormat
{
    [JsonProperty("format")]
    public required string Format { get; set; }
}