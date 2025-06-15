using DbPerformanceComparison.Infrastructure.Mongo;
using DbPerformanceComparison.Infrastructure.Postgres;
using DbPerformanceComparison.Monitoring;
using DbPerformanceComparison.Repositories.Interfaces;
using DbPerformanceComparison.Repositories.Mongo;
using DbPerformanceComparison.Repositories.Postgres;
using DbPerformanceComparison.Services.ConfigurationBuilder;
using DbPerformanceComparison.Services.Parser;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections;
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

        static public (List<Event> events, List<Athlete> athletes, List<Result> results) EscalateElements(List<Event> events, List<Athlete> athletes, List<Result> results, int scaleFactor = 1)
        {
            var allEvents = new List<Event>(events);
            var allAthletes = new List<Athlete>(athletes);
            var allResults = new List<Result>(results);

            if (scaleFactor > 1)
            {
                for (int s = 1; s < scaleFactor; s++)
                {
                    var eventMap = new Dictionary<Guid, Event>();
                    foreach (var originalEvent in events)
                    {
                        var clonedEvent = originalEvent.Clone();
                        clonedEvent.Id = Guid.NewGuid();
                        eventMap[originalEvent.Id] = clonedEvent;
                        allEvents.Add(clonedEvent);
                    }

                    var athleteMap = new Dictionary<Guid, Athlete>();
                    foreach (var originalAthlete in athletes)
                    {
                        var clonedAthlete = originalAthlete.Clone();
                        clonedAthlete.Id = Guid.NewGuid();
                        athleteMap[originalAthlete.Id] = clonedAthlete;
                        allAthletes.Add(clonedAthlete);
                    }

                    foreach (var originalResult in results)
                    {
                        var clonedResult = originalResult.Clone();
                        clonedResult.Id = Guid.NewGuid();
                        if (originalResult.AthleteId.HasValue && athleteMap.TryGetValue(originalResult.AthleteId.Value, out var newAthlete))
                        {
                            clonedResult.AthleteId = newAthlete.Id;
                            clonedResult.Athlete = newAthlete;
                        }
                        if (originalResult.EventId.HasValue && eventMap.TryGetValue(originalResult.EventId.Value, out var newEvent))
                        {
                            clonedResult.EventId = newEvent.Id;
                            clonedResult.Event = newEvent;
                        }
                        allResults.Add(clonedResult);
                    }
                }
            }
            return (allEvents, allAthletes, allResults);
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

                athletes = (await performanceMonitor.MeasureGetAllAsync(postgresAthleteRepository, postgresAthleteRepository.GetAllAsync))?.ToList() ?? new List<Athlete>();
                athletes = (await performanceMonitor.MeasureGetAllAsync(mongoAthleteRepository, mongoAthleteRepository.GetAllAsync))?.ToList() ?? new List<Athlete>();
                events = (await performanceMonitor.MeasureGetAllAsync(postgresEventRepository, postgresEventRepository.GetAllAsync))?.ToList() ?? new List<Event>();
                events = (await performanceMonitor.MeasureGetAllAsync(mongoEventRepository, mongoEventRepository.GetAllAsync))?.ToList() ?? new List<Event>();
                results = (await performanceMonitor.MeasureGetAllAsync(postgresResultRepository, postgresResultRepository.GetAllAsync))?.ToList() ?? new List<Result>();
                results = (await performanceMonitor.MeasureGetAllAsync(mongoResultRepository, mongoResultRepository.GetAllAsync))?.ToList() ?? new List<Result>();

                bool res = await performanceMonitor.MeasureUpdateAsync(postgresAthleteRepository, athletes.First(), postgresAthleteRepository.UpdateAsync);
                res = await performanceMonitor.MeasureUpdateAsync(mongoAthleteRepository, athletes.First(), mongoAthleteRepository.UpdateAsync);
                res = await performanceMonitor.MeasureUpdateAsync(postgresEventRepository, events.First(), postgresEventRepository.UpdateAsync);
                res = await performanceMonitor.MeasureUpdateAsync(mongoEventRepository, events.First(), mongoEventRepository.UpdateAsync);
                res = await performanceMonitor.MeasureUpdateAsync(postgresResultRepository, results.First(), postgresResultRepository.UpdateAsync);
                res = await performanceMonitor.MeasureUpdateAsync(mongoResultRepository, results.First(), mongoResultRepository.UpdateAsync);

                res = await performanceMonitor.MeasureDeleteAsync(postgresAthleteRepository, athletes.First().Id, postgresAthleteRepository.DeleteAsync);
                res = await performanceMonitor.MeasureDeleteAsync(mongoAthleteRepository, athletes.First().Id, mongoAthleteRepository.DeleteAsync);
                res = await performanceMonitor.MeasureDeleteAsync(postgresEventRepository, events.First().Id, postgresEventRepository.DeleteAsync);
                res = await performanceMonitor.MeasureDeleteAsync(mongoEventRepository, events.First().Id, mongoEventRepository.DeleteAsync);
                res = await performanceMonitor.MeasureDeleteAsync(postgresResultRepository, results.First().Id, postgresResultRepository.DeleteAsync);
                res = await performanceMonitor.MeasureDeleteAsync(mongoResultRepository, results.First().Id, mongoResultRepository.DeleteAsync);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
