using DbPerformanceComparison.Infrastructure.Mongo;
using DbPerformanceComparison.Infrastructure.Postgres;
using DbPerformanceComparison.Repositories.Interfaces;
using DbPerformanceComparison.Repositories.Mongo;
using DbPerformanceComparison.Repositories.Postgres;
using DbPerformanceComparison.Services.ConfigurationBuilder;
using DbPerformanceComparison.Services.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbPerformanceComparison.Monitoring
{
    public class BenchmarkRunner
    {
        private List<Event> _events;
        private List<Athlete> _athletes;
        private List<Result> _results;

        private AthleteRepository _postgresAthleteRepository;
        private EventRepository _postgresEventRepository;
        private ResultRepository _postgresResultRepository;
        private MongoRepository<Athlete> _mongoAthleteRepository;
        private MongoRepository<Result> _mongoResultRepository;
        private MongoRepository<Event> _mongoEventRepository;
        private PerformanceMonitor _performanceMonitor;
        private int _repetitions;
        private int _scale;

        private void ParseInputFiles()
        {
            string eventsPath = Path.Combine("Input", "events.csv");
            string resultsPath = Path.Combine("Input", "results.csv");

            EventCsvParser eventParser = new();
            ResultCsvParser resultParser = new();

            _events = eventParser.Parse(eventsPath);
            (_athletes, _results) = resultParser.Parse(resultsPath, _events);
        }

        private void EscalateElements()
        {
            var allEvents = new List<Event>(_events);
            var allAthletes = new List<Athlete>(_athletes);
            var allResults = new List<Result>(_results);

            if (_scale > 1)
            {
                for (int i = 1; i < _scale; i++)
                {
                    var eventMap = new Dictionary<Guid, Event>();
                    foreach (var originalEvent in _events)
                    {
                        var clonedEvent = originalEvent.Clone();
                        clonedEvent.Id = Guid.NewGuid();
                        eventMap[originalEvent.Id] = clonedEvent;
                        allEvents.Add(clonedEvent);
                    }

                    var athleteMap = new Dictionary<Guid, Athlete>();
                    foreach (var originalAthlete in _athletes)
                    {
                        var clonedAthlete = originalAthlete.Clone();
                        clonedAthlete.Id = Guid.NewGuid();
                        athleteMap[originalAthlete.Id] = clonedAthlete;
                        allAthletes.Add(clonedAthlete);
                    }

                    foreach (var originalResult in _results)
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

            _events = allEvents;
            _athletes = allAthletes;
            _results = allResults;
        }

        public async Task Init(int scale = 1, int repetitions = 1)
        {
            _repetitions = repetitions;
            _scale = scale;

            ParseInputFiles();
            EscalateElements();

            //POSTGRES DB
            ConfigurationBuilderService configurationBuilderService = new();
            string postgresConnectionString = configurationBuilderService.GetPostgresConnectionString();

            PostgresService postgresService = new(postgresConnectionString);
            TableInitializer initializer = new(postgresService);
            await initializer.InitializeTablesAsync(true);

            _postgresAthleteRepository = new(postgresService);
            _postgresEventRepository = new(postgresService);
            _postgresResultRepository = new(postgresService);

            //MONGO DB
            string mongoConnectionString = configurationBuilderService.GetMongoConnectionString();
            string mongoDatabaseName = configurationBuilderService.GetMongoDatabaseName();

            MongoService mongoService = new(mongoConnectionString, mongoDatabaseName, true);

            _mongoAthleteRepository = new(mongoService);
            _mongoResultRepository = new(mongoService);
            _mongoEventRepository = new(mongoService);

            _performanceMonitor = new(scale);
        }

        public async Task RunTestsAsync()
        {
            for (int i = 0; i < _repetitions; i++)
            {
                await _performanceMonitor.MeasureAddManyAsync(_postgresAthleteRepository, _athletes, _postgresAthleteRepository.AddManyAsync);
                await _performanceMonitor.MeasureAddManyAsync(_mongoAthleteRepository, _athletes, _mongoAthleteRepository.AddManyAsync);
                await _performanceMonitor.MeasureAddManyAsync(_postgresEventRepository, _events, _postgresEventRepository.AddManyAsync);
                await _performanceMonitor.MeasureAddManyAsync(_mongoEventRepository, _events, _mongoEventRepository.AddManyAsync);
                await _performanceMonitor.MeasureAddManyAsync(_postgresResultRepository, _results, _postgresResultRepository.AddManyAsync);
                await _performanceMonitor.MeasureAddManyAsync(_mongoResultRepository, _results, _mongoResultRepository.AddManyAsync);

                _ = await _performanceMonitor.MeasureGetByIdAsync(_postgresAthleteRepository, _athletes.First().Id, _postgresAthleteRepository.GetByIdAsync);
                _ = await _performanceMonitor.MeasureGetByIdAsync(_mongoAthleteRepository, _athletes.First().Id, _mongoAthleteRepository.GetByIdAsync);
                _ = await _performanceMonitor.MeasureGetByIdAsync(_postgresEventRepository, _events.First().Id, _postgresEventRepository.GetByIdAsync);
                _ = await _performanceMonitor.MeasureGetByIdAsync(_mongoEventRepository, _events.First().Id, _mongoEventRepository.GetByIdAsync);
                _ = await _performanceMonitor.MeasureGetByIdAsync(_postgresResultRepository, _results.First().Id, _postgresResultRepository.GetByIdAsync);
                _ = await _performanceMonitor.MeasureGetByIdAsync(_mongoResultRepository, _results.First().Id, _mongoResultRepository.GetByIdAsync);

                _ = (await _performanceMonitor.MeasureGetAllAsync(_postgresAthleteRepository, _postgresAthleteRepository.GetAllAsync))?.ToList() ?? new List<Athlete>();
                _ = (await _performanceMonitor.MeasureGetAllAsync(_mongoAthleteRepository, _mongoAthleteRepository.GetAllAsync))?.ToList() ?? new List<Athlete>();
                _ = (await _performanceMonitor.MeasureGetAllAsync(_postgresEventRepository, _postgresEventRepository.GetAllAsync))?.ToList() ?? new List<Event>();
                _ = (await _performanceMonitor.MeasureGetAllAsync(_mongoEventRepository, _mongoEventRepository.GetAllAsync))?.ToList() ?? new List<Event>();
                _ = (await _performanceMonitor.MeasureGetAllAsync(_postgresResultRepository, _postgresResultRepository.GetAllAsync))?.ToList() ?? new List<Result>();
                _ = (await _performanceMonitor.MeasureGetAllAsync(_mongoResultRepository, _mongoResultRepository.GetAllAsync))?.ToList() ?? new List<Result>();

                _ = await _performanceMonitor.MeasureUpdateAsync(_postgresAthleteRepository, _athletes.First(), _postgresAthleteRepository.UpdateAsync);
                _ = await _performanceMonitor.MeasureUpdateAsync(_mongoAthleteRepository, _athletes.First(), _mongoAthleteRepository.UpdateAsync);
                _ = await _performanceMonitor.MeasureUpdateAsync(_postgresEventRepository, _events.First(), _postgresEventRepository.UpdateAsync);
                _ = await _performanceMonitor.MeasureUpdateAsync(_mongoEventRepository, _events.First(), _mongoEventRepository.UpdateAsync);
                _ = await _performanceMonitor.MeasureUpdateAsync(_postgresResultRepository, _results.First(), _postgresResultRepository.UpdateAsync);
                _ = await _performanceMonitor.MeasureUpdateAsync(_mongoResultRepository, _results.First(), _mongoResultRepository.UpdateAsync);

                _ = await _performanceMonitor.MeasureDeleteAsync(_postgresResultRepository, _results.First().Id, _postgresResultRepository.DeleteAsync);
                _ = await _performanceMonitor.MeasureDeleteAsync(_mongoResultRepository, _results.First().Id, _mongoResultRepository.DeleteAsync);
                _ = await _performanceMonitor.MeasureDeleteAsync(_postgresAthleteRepository, _athletes.First().Id, _postgresAthleteRepository.DeleteAsync);
                _ = await _performanceMonitor.MeasureDeleteAsync(_mongoAthleteRepository, _athletes.First().Id, _mongoAthleteRepository.DeleteAsync);
                _ = await _performanceMonitor.MeasureDeleteAsync(_postgresEventRepository, _events.First().Id, _postgresEventRepository.DeleteAsync);
                _ = await _performanceMonitor.MeasureDeleteAsync(_mongoEventRepository, _events.First().Id, _mongoEventRepository.DeleteAsync);

                _ = await _performanceMonitor.MeasureDeleteAllAsync(_postgresResultRepository, _postgresResultRepository.DeleteAllAsync);
                _ = await _performanceMonitor.MeasureDeleteAllAsync(_mongoResultRepository, _mongoResultRepository.DeleteAllAsync);
                _ = await _performanceMonitor.MeasureDeleteAllAsync(_postgresAthleteRepository, _postgresAthleteRepository.DeleteAllAsync);
                _ = await _performanceMonitor.MeasureDeleteAllAsync(_mongoAthleteRepository, _mongoAthleteRepository.DeleteAllAsync);
                _ = await _performanceMonitor.MeasureDeleteAllAsync(_postgresEventRepository, _postgresEventRepository.DeleteAllAsync);
                _ = await _performanceMonitor.MeasureDeleteAllAsync(_mongoEventRepository, _mongoEventRepository.DeleteAllAsync);
            }
        }
    }
}
