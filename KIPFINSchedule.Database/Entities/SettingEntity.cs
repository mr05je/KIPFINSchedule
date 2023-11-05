// ReSharper disable InconsistentNaming

using System.ComponentModel.DataAnnotations.Schema;

namespace KIPFINSchedule.Database.Entities;

public class SettingEntity : IEntity<long>
{
    public long Id { get; set; }

    [Column("monday_time")] public TimeSpan MondayTime { get; set; } = TimeSpan.FromMinutes(360);
    [Column("tuesday_time")] public TimeSpan TuesdayTime { get; set; } = TimeSpan.FromMinutes(360);
    [Column("wednesday_time")] public TimeSpan WednesdayTime { get; set; } = TimeSpan.FromMinutes(360);
    [Column("thursday_time")] public TimeSpan ThursdayTime { get; set; } = TimeSpan.FromMinutes(360);
    [Column("friday_time")] public TimeSpan FridayTime { get; set; } = TimeSpan.FromMinutes(360);

    [Column("use_monday_time")]
    public bool UseMondayTime { get; set; }
    [Column("gps_message_format")]
    
    public string? GPSMessageFormat { get; set; }
    [Column("as_message_format")]
    public string? ASMessageFormat { get; set; }
    [Column("item_format")]
    public string? ItemFormat { get; set; }
}