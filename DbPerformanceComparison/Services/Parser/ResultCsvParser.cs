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
        private readonly Dictionary<string, Athlete> _athleteMap = new();

        public (List<Athlete> athletes, List<Result> results) Parse(string csvPath, List<Event> existingEvents)
        {
            var athletes = new List<Athlete>();
            var results = new List<Result>();

            using var reader = new StreamReader(csvPath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Read();
            csv.ReadHeader();

            //local_time,sex,event,round,startlist_url,results_url,summary_url,points_url
            //pos,bib,name,country,mark
            while (csv.Read())
            {
                var name = csv.GetField("name");
                var country = csv.GetField("country");
                var sex = csv.GetField("sex");
                var eventName = csv.GetField("event");
                var round = csv.GetField("round");

                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(eventName))
                    continue;

                var athleteKey = $"{name}|{country}";
                if (!_athleteMap.TryGetValue(athleteKey, out var athlete))
                {
                    athlete = new Athlete
                    {
                        Name = name,
                        Country = country,
                        Sex = sex
                    };
                    _athleteMap[athleteKey] = athlete;
                    athletes.Add(athlete);
                }

                var ev = existingEvents.FirstOrDefault(e =>
                    e.Name == eventName && e.Sex == sex && e.Round == round);

                if (ev == null)
                    continue;

                int? TryParseDoubleAsInt(string field)
                    => double.TryParse(csv.GetField(field), NumberStyles.Any, CultureInfo.InvariantCulture, out var val) ? (int)val : null;

                DateTime? TryParseDate(string field)
                    => DateTime.TryParse(csv.GetField(field), out var val) ? val : null;

                var result = new Result
                {
                    Athlete = athlete,
                    AthleteId = athlete.Id,
                    Event = ev,
                    EventId = ev.Id,
                    Position = TryParseDoubleAsInt("pos"),
                    Bib = TryParseDoubleAsInt("bib"),
                    Mark = TryParseDate("mark")
                };

                results.Add(result);
            }

            return (athletes, results);
        }
    }
}
