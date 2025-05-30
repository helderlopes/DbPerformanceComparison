using DbPerformanceComparison.Infrastructure.Mongo;
using DbPerformanceComparison.Infrastructure.Postgres;
using DbPerformanceComparison.Monitoring;
using DbPerformanceComparison.Repositories.Interfaces;
using DbPerformanceComparison.Repositories.Mongo;
using DbPerformanceComparison.Repositories.Postgres;
using DbPerformanceComparison.Services.ConfigurationBuilder;
using DbPerformanceComparison.Services.Parser;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Diagnostics;

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

                //MONGO DB
                string mongoConnectionString = configurationBuilderService.GetMongoConnectionString();
                string mongoDatabaseName = configurationBuilderService.GetMongoDatabaseName();

                MongoService mongoService = new(mongoConnectionString, mongoDatabaseName, true);

                MongoRepository<Athlete> mongoAthleteRepository = new(mongoService);
                MongoRepository<Result> mongoResultRepository = new(mongoService);
                MongoRepository<Event> mongoEventRepository = new(mongoService);

                PerformanceMonitor performanceMonitor = new();

                await performanceMonitor.MeasureAddManyAsync(postgresAthleteRepository, athletes, postgresAthleteRepository.AddManyAsync);
                await performanceMonitor.MeasureAddManyAsync(mongoAthleteRepository, athletes, mongoAthleteRepository.AddManyAsync);
                await performanceMonitor.MeasureAddManyAsync(postgresEventRepository, events, postgresEventRepository.AddManyAsync);
                await performanceMonitor.MeasureAddManyAsync(mongoEventRepository, events, mongoEventRepository.AddManyAsync);
                await performanceMonitor.MeasureAddManyAsync(postgresResultRepository, results, postgresResultRepository.AddManyAsync);
                await performanceMonitor.MeasureAddManyAsync(mongoResultRepository, results, mongoResultRepository.AddManyAsync);

                var athlete = await performanceMonitor.MeasureGetByIdAsync(postgresAthleteRepository, athletes.First().Id, postgresAthleteRepository.GetByIdAsync);
                athlete = await performanceMonitor.MeasureGetByIdAsync(mongoAthleteRepository, athletes.First().Id, mongoAthleteRepository.GetByIdAsync);
                var firstEvent = await performanceMonitor.MeasureGetByIdAsync(postgresEventRepository, events.First().Id, postgresEventRepository.GetByIdAsync);
                firstEvent = await performanceMonitor.MeasureGetByIdAsync(mongoEventRepository, events.First().Id, mongoEventRepository.GetByIdAsync);
                var result = await performanceMonitor.MeasureGetByIdAsync(postgresResultRepository, results.First().Id, postgresResultRepository.GetByIdAsync);
                result = await performanceMonitor.MeasureGetByIdAsync(mongoResultRepository, results.First().Id, mongoResultRepository.GetByIdAsync);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
