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

    public Event Clone()
    {
        return new Event
        {
            Id = this.Id,
            Name = this.Name,
            EventTime = this.EventTime,
            Sex = this.Sex,
            Round = this.Round,
            StartListUrl = this.StartListUrl,
            ResultsUrl = this.ResultsUrl,
            SummaryUrl = this.SummaryUrl,
            PointsUrl = this.PointsUrl
        };
    }
}
