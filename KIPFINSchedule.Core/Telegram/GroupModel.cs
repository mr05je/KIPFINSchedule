using Newtonsoft.Json;

namespace KIPFINSchedule.Core.Telegram;

public class GroupModel
{
    
    [JsonProperty("items")]
    public Dictionary<string, List<string>>? Groups { get; set; }
}