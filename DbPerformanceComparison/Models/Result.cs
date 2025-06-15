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
    [BsonRepresentation(BsonType.String)]
    public Guid? AthleteId { get; set; }
    public Event? Event { get; set; }
    [BsonRepresentation(BsonType.String)]
    public Guid? EventId { get; set; }
    public int? Position { get; set; }
    public int? Bib { get; set; }
    public TimeSpan? Mark { get; set; }

    public Result Clone()
    {
        return new Result
        {
            Id = this.Id,
            Athlete = this.Athlete,
            AthleteId = this.AthleteId,
            Event = this.Event,
            EventId = this.EventId,
            Position = this.Position,
            Bib = this.Bib,
            Mark = this.Mark
        };
    }
}
