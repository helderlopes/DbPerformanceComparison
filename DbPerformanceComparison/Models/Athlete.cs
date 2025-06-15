using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations.Schema;

public class Athlete
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    [Column("Id")] 
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Sex { get; set; }
    public string? Country { get; set; }

    public Athlete Clone()
    {
        return new Athlete
        {
            Id = this.Id,
            Name = this.Name,
            Sex = this.Sex,
            Country = this.Country
        };
    }
}