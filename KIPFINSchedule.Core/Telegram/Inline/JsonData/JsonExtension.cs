using Newtonsoft.Json;

namespace KIPFINSchedule.Core.Telegram.Inline.JsonData;

public static class JsonExtension
{
    private static readonly JsonSerializerSettings Settings = new()
        { NullValueHandling = NullValueHandling.Ignore };

    public static string SerializeObject<T>(T obj) => JsonConvert.SerializeObject(obj, Formatting.None, Settings);
}