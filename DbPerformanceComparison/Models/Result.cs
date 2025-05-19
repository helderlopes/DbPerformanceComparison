public class Result
{
    public Guid Id { get; set; }
    public Athlete? Athlete { get; set; }
    public Guid? AthleteId { get; set; }
    public Event? Event { get; set; }
    public Guid? EventId { get; set; }
    public int? Position { get; set; }
    public int? Bib { get; set; }
    public TimeSpan? Mark { get; set; }
}
