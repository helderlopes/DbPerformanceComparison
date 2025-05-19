using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;

namespace DbPerformanceComparison.Services.Parser
{
    public class EventCsvParser
    {
        public List<Event> Parse(string filePath)
        {
            using StreamReader reader = new StreamReader(filePath);
            using CsvReader csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null
            });

            List<Event> records = new List<Event>();

            csv.Read();
            csv.ReadHeader();

            while (csv.Read())
            {
                records.Add(new Event
                {
                    Id = Guid.NewGuid(),
                    EventTime = TimeSpan.TryParse(csv.GetField("local_time"), out var parsedDate) ? parsedDate : null,
                    Sex = csv.GetField("sex"),
                    Name = csv.GetField("event"),
                    Round = csv.GetField("round"),
                    StartListUrl = csv.GetField("startlist_url"),
                    ResultsUrl = csv.GetField("results_url"),
                    SummaryUrl = csv.GetField("summary_url"),
                    PointsUrl = csv.GetField("points_url")
                });
            }

            return records;
        }
    }
}
