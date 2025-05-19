using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations.Schema;

public class Result
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    [Column("Id")]
    public Guid Id { get; set; }
    public Athlete? Athlete { get; set; }
    public Guid? AthleteId { get; set; }
    public Event? Event { get; set; }
    public Guid? EventId { get; set; }
    public int? Position { get; set; }
    public int? Bib { get; set; }
    public TimeSpan? Mark { get; set; }
}
