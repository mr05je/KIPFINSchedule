using System.ComponentModel.DataAnnotations.Schema;

namespace KIPFINSchedule.Database.Entities;

public class ProfileEntity : IEntity<long>
{
    public long Id { get; set; }
    [Column("avatar_url")]
    public string? AvatarUrl { get; set; }
    [Column("username")]
    public string? Username { get; set; }
}