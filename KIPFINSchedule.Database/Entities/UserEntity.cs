using System.ComponentModel.DataAnnotations.Schema;

namespace KIPFINSchedule.Database.Entities;

public class UserEntity : IEntity<long>
{
    public long Id { get; set; }
    
    [Column("group")]
    public string? Group { get; set; }
    [Column("autoschedule")]
    public bool AutoSchedule { get; set; }
    [Column("subscription_id")]
    public SubscriptionEntity? Subscription { get; set; }
    [Column("setting_id")]
    public SettingEntity? Setting { get; set; }
}