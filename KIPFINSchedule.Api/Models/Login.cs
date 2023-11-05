using Newtonsoft.Json;

namespace KIPFINSchedule.Api.Models;

public class LoginReq
{
    [JsonProperty("id")]
    public required string Id { get; set; }
    [JsonProperty("first_name")]
    public required string FirstName { get; set; }
    [JsonProperty("last_name")]
    public required string LastName { get; set; }
    [JsonProperty("username")]
    public required string Username { get; set; }
    [JsonProperty("photo_url")]
    public required string PhotoUrl { get; set; }
    [JsonProperty("auth_date")]
    public required string AuthDate { get; set; }
    [JsonProperty("hash")]
    public required string Hash { get; set; }
}

public class LoginRes
{
    [JsonProperty("token")]
    public required string Token { get; set; }
}