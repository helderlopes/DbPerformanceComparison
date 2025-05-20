using DbPerformanceComparison.Infrastructure.Mongo;
using DbPerformanceComparison.Infrastructure.Postgres;
using DbPerformanceComparison.Repositories.Interfaces;
using DbPerformanceComparison.Repositories.Mongo;
using DbPerformanceComparison.Repositories.Postgres;
using DbPerformanceComparison.Services.ConfigurationBuilder;
using DbPerformanceComparison.Services.Parser;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace DbPerformanceComparison
{
    internal class Program
    {
        static public (List<Event> events, List<Athlete> athletes, List<Result> results) ParseInputFiles()
        {
            string eventsPath = Path.Combine("Input", "events.csv");
            string resultsPath = Path.Combine("Input", "results.csv");

            EventCsvParser eventParser = new();
            ResultCsvParser resultParser = new();

            List<Event> events = eventParser.Parse(eventsPath);
            (List<Athlete> athletes, List<Result> results) lists = resultParser.Parse(resultsPath, events);

            return (events, lists.athletes, lists.results);
        }

        static async Task Main(string[] args)
        {
            try
            {
                (List<Event> events, List<Athlete> athletes, List<Result> results) = ParseInputFiles();

                //POSTGRES DB
                ConfigurationBuilderService configurationBuilderService = new();
                string postgresConnectionString = configurationBuilderService.GetPostgresConnectionString();
                
                PostgresService postgresService = new(postgresConnectionString);
                TableInitializer initializer = new(postgresService);
                await initializer.InitializeTablesAsync(true);

                AthleteRepository postgresAthleteRepository = new(postgresService);
                EventRepository postgresEventRepository = new(postgresService);
                ResultRepository postgresResultRepository = new(postgresService);

                await postgresAthleteRepository.AddManyAsync(athletes);
                await postgresEventRepository.AddManyAsync(events);
                await postgresResultRepository.AddManyAsync(results);

                athletes = (await postgresAthleteRepository.GetAllAsync()).ToList();
                events = (await postgresEventRepository.GetAllAsync()).ToList();
                results = (await postgresResultRepository.GetAllAsync()).ToList();

                //MONGO DB
                string mongoConnectionString = configurationBuilderService.GetMongoConnectionString();
                string mongoDatabaseName = configurationBuilderService.GetMongoDatabaseName();

                MongoService mongoService = new(mongoConnectionString, mongoDatabaseName, true);

                MongoRepository<Athlete> mongoAthleteRepository = new(mongoService);
                MongoRepository<Result> mongoResultRepository = new(mongoService);
                MongoRepository<Event> mongoEventRepository = new(mongoService);

                await mongoAthleteRepository.AddManyAsync(athletes);
                await mongoEventRepository.AddManyAsync(events);
                await mongoResultRepository.AddManyAsync(results);

                athletes = (await mongoAthleteRepository.GetAllAsync()).ToList();
                events = (await mongoEventRepository.GetAllAsync()).ToList();
                results = (await mongoResultRepository.GetAllAsync()).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
