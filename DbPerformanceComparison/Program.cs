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

                ResultRepository resultRepository = new(postgresService);



                await resultRepository.AddManyAsync(lists.results);

                List<Result> listResults = (await resultRepository.GetAllAsync()).ToList();

                foreach (Result entity in listResults)
                {
                    await athleteRepository.DeleteAsync(entity.Id);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
