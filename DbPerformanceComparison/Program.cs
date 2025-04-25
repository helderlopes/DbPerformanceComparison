using DbPerformanceComparison.Services.Parser;
using Microsoft.Extensions.Configuration;

namespace DbPerformanceComparison
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string eventsPath = Path.Combine("Input", "events.csv");
            string resultsPath = Path.Combine("Input", "results.csv");

            EventCsvParser eventParser = new ();
            ResultCsvParser resultParser = new ();

            try
            {
                List<Event> events = eventParser.Parse(eventsPath);
                (List<Athlete> athletes, List<Result> results) lists = resultParser.Parse(resultsPath, events);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading csv: {ex.Message}");
            }
        }
    }
}
