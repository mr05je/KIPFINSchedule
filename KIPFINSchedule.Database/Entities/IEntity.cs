using System.ComponentModel.DataAnnotations.Schema;

namespace KIPFINSchedule.Database.Entities;

public interface IEntity<T>
{
    [Column("id")]
    public T Id { get; set; }
}