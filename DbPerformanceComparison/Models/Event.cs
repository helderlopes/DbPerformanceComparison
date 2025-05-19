using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations.Schema;

public class Event
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    [Column("Id")]
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public TimeSpan? EventTime { get; set; }
    public string? Sex { get; set; }
    public string? Round { get; set; }
    public string? StartListUrl { get; set; }
    public string? ResultsUrl { get; set; }
    public string? SummaryUrl { get; set; }
    public string? PointsUrl { get; set; }
}
