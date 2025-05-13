using DbPerformanceComparison.Infrastructure.Postgres;
using DbPerformanceComparison.Repositories.Postgres;
using DbPerformanceComparison.Services.Parser;
using Microsoft.Extensions.Configuration;

namespace DbPerformanceComparison
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string eventsPath = Path.Combine("Input", "events.csv");
            string resultsPath = Path.Combine("Input", "results.csv");

            EventCsvParser eventParser = new();
            ResultCsvParser resultParser = new();

            try
            {
                List<Event> events = eventParser.Parse(eventsPath);
                (List<Athlete> athletes, List<Result> results) lists = resultParser.Parse(resultsPath, events);

                var config = new ConfigurationBuilder()
                    .AddEnvironmentVariables()
                    .AddJsonFile("appsettings.json", optional: true)
                    .Build();

                string connectionString = $"Host={config["Postgres:Host"]};Port={config["Postgres:Port"]};Username={config["Postgres:Username"]};Password={config["Postgres:Password"]};Database={config["Postgres:Database"]}";

                PostgresService postgresService = new(connectionString);

                TableInitializer initializer = new(postgresService);

                await initializer.InitializeTablesAsync(true);

                AthleteRepository athleteRepository = new(postgresService);

                await athleteRepository.AddManyAsync(lists.athletes);

                List<Athlete> listAthletes = (await athleteRepository.GetAllAsync()).ToList();

                EventRepository eventRepository = new(postgresService);

                await eventRepository.AddManyAsync(events);

                List<Event> listEvents = (await eventRepository.GetAllAsync()).ToList();

                //get athlete and event ids and add to lists.results
                Dictionary<string, int> athleteDict = listAthletes.ToDictionary(a => $"{a.Name}|{a.Country}", a => a.Id);
                Dictionary<string, int> eventDict = listEvents
                                                        .Where(e => e.Name != null)
                                                        .DistinctBy(e => e.Name)
                                                        .ToDictionary(e => e.Name!, e => e.Id);

                foreach (var result in lists.results)
                {
                    if (result.Athlete?.Name is not null && result.Athlete?.Country is not null)
                    {
                        string athleteKey = $"{result.Athlete.Name}|{result.Athlete.Country}";

                        if (athleteDict.TryGetValue(athleteKey, out int athleteId))
                        {
                            result.AthleteId = athleteId;
                        }
                        else
                        {
                            Console.WriteLine($"Athlete not found: {result.Athlete.Name} from {result.Athlete.Country}");
                        }
                    }

                    if (result.Event?.Name is not null && eventDict.TryGetValue(result.Event.Name, out int eventId))
                    {
                        result.EventId = eventId;
                    }
                    else
                    {
                        Console.WriteLine($"Event not found: {result.Event?.Name}");
                    }
                }

                ResultRepository resultRepository = new(postgresService);

                await resultRepository.AddManyAsync(lists.results);

                List<Result> listResults = (await resultRepository.GetAllAsync()).ToList();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
