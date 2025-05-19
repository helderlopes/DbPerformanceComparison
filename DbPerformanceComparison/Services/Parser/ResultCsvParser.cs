using CsvHelper.Configuration;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbPerformanceComparison.Services.Parser
{
    public class ResultCsvParser
    {
        public (List<Athlete> athletes, List<Result> results) Parse(string csvPath, List<Event> existingEvents)
        {
            Dictionary<string, Athlete> athleteMap = new();
            List<Athlete> athletes = new();
            List<Result> results = new();

            using StreamReader reader = new StreamReader(csvPath);
            using CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Read();
            csv.ReadHeader();

            while (csv.Read())
            {
                var name = csv.GetField("name");
                var country = csv.GetField("country");
                var sex = csv.GetField("sex");
                var eventName = csv.GetField("event");
                var round = csv.GetField("round");

                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(eventName))
                { 
                    continue;
                }

                var athleteKey = $"{name}|{country}";
                if (!athleteMap.TryGetValue(athleteKey, out var athlete))
                {
                    athlete = new Athlete
                    {
                        Id = Guid.NewGuid(),
                        Name = name,
                        Country = country,
                        Sex = sex
                    };
                    athleteMap[athleteKey] = athlete;
                    athletes.Add(athlete);
                }

                Event? currentEvent = existingEvents.FirstOrDefault(e =>
                    e.Name == eventName && e.Sex == sex && e.Round == round);

                if (currentEvent == null)
                {
                    continue;
                }

                int? TryParseDoubleAsInt(string field)
                    => double.TryParse(csv.GetField(field), NumberStyles.Any, CultureInfo.InvariantCulture, out var val) ? (int)val : null;

                TimeSpan? TryParseTime(string field)
                    => TimeSpan.TryParse(csv.GetField(field), out var val) ? val : null;

                Result result = new Result
                {
                    Id = Guid.NewGuid(),
                    Athlete = athlete,
                    AthleteId = athlete.Id, 
                    Event = currentEvent,
                    EventId = currentEvent.Id, 
                    Position = TryParseDoubleAsInt("pos"),
                    Bib = TryParseDoubleAsInt("bib"),
                    Mark = TryParseTime("mark")
                };

                results.Add(result);
            }

            return (athletes, results);
        }
    }
}
