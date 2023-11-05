using System.ComponentModel.DataAnnotations.Schema;

namespace KIPFINSchedule.Database.Entities;

public class SubscriptionEntity : IEntity<long>
{
    public long Id { get; set; }
    [Column("provider_payment_charge_id")]
    public required string ProviderPaymentChargeId { get; set; }
    [Column("expire_at")]
    public required DateTime ExpireAt { get; set; }
    [Column("notified")]
    public bool Notified { get; set; }
}