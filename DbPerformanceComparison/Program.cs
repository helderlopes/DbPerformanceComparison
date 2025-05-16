using DbPerformanceComparison.Infrastructure.Postgres;
using DbPerformanceComparison.Repositories.Postgres;
using DbPerformanceComparison.Services.ConfigurationBuilder;
using DbPerformanceComparison.Services.Parser;
using Microsoft.Extensions.Configuration;

namespace DbPerformanceComparison
{
    internal class Program
    {
        static public (List<Athlete> athletes, List<Result> results, List<Event> events) ParseInputFiles()
        {
            string eventsPath = Path.Combine("Input", "events.csv");
            string resultsPath = Path.Combine("Input", "results.csv");

            EventCsvParser eventParser = new();
            ResultCsvParser resultParser = new();

            List<Event> events = eventParser.Parse(eventsPath);
            (List<Athlete> athletes, List<Result> results) lists = resultParser.Parse(resultsPath, events);

            return (lists.athletes, lists.results, events);
        }

        static async Task<List<Athlete>> SeedAthletesAsync(PostgresService postgresService, List<Athlete> athletes)
        {
            AthleteRepository athleteRepository = new(postgresService);

            await athleteRepository.AddManyAsync(athletes);

            return (await athleteRepository.GetAllAsync()).ToList();
        }

        static async Task<List<Event>> SeedEventsAsync(PostgresService postgresService, List<Event> events)
        {
            EventRepository eventRepository = new(postgresService);

            await eventRepository.AddManyAsync(events);

            return (await eventRepository.GetAllAsync()).ToList();
        }

        static void MapResultReferences(List<Result> results, List<Athlete> athletes, List<Event> events)
        {
            foreach (var result in results)
            {
                if (result.Athlete != null)
                {
                    result.AthleteId = athletes.FirstOrDefault(a => a.Name == result.Athlete.Name && a.Country == result.Athlete.Country)?.Id;
                }
                if (result.Event != null)
                {
                    result.EventId = events.FirstOrDefault(e => e.Name == result.Event.Name)?.Id;
                }
            }
        }

        static async Task<List<Result>> SeedResultsAsync(PostgresService postgresService, List<Result> results)
        {
            ResultRepository resultRepository = new(postgresService);

            await resultRepository.AddManyAsync(results);

            return (await resultRepository.GetAllAsync()).ToList();
        }

        static async Task Main(string[] args)
        {
            try
            {
                ConfigurationBuilderService configurationBuilderService = new();
                string postgresConnectionString = configurationBuilderService.GetPostgresConnectionString();
                
                PostgresService postgresService = new(postgresConnectionString);
                TableInitializer initializer = new(postgresService);
                await initializer.InitializeTablesAsync(true);

                (List<Athlete> athletes, List<Result> results, List<Event> events) = ParseInputFiles();

                athletes = await SeedAthletesAsync(postgresService, athletes);
                events = await SeedEventsAsync(postgresService, events);

                MapResultReferences(results, athletes, events);

                results = await SeedResultsAsync(postgresService, results);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
