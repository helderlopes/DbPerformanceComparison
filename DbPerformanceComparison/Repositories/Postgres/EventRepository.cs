using DbPerformanceComparison.Infrastructure.Postgres;
using DbPerformanceComparison.Repositories.Interfaces;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbPerformanceComparison.Repositories.Postgres
{
    public class EventRepository : IRepository<Event>
    {
        private readonly PostgresService _service;

        public EventRepository(PostgresService service)
        {
            _service = service;
        }

        public async Task<int> AddAsync(Event entity)
        {
            NpgsqlConnection connection = await _service.GetConnectionAsync();
            string query =  "INSERT INTO Events (Name, EventTime, Sex, Round, StartListurl, ResultsUrl, SummaryUrl, PointsUrl) " +
                            "VALUES (@Name, @EventTime, @Sex, @Round, @StartListurl, @ResultsUrl, @SummaryUrl, @PointsUrl) RETURNING Id";

            await using NpgsqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@Name", entity.Name ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@EventTime", entity.EventTime ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Sex", entity.Sex ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Round", entity.Round ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@StartListUrl", entity.StartListUrl ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ResultsUrl", entity.ResultsUrl ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@SummaryUrl", entity.SummaryUrl ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@PointsUrl", entity.PointsUrl ?? (object)DBNull.Value);

            var result = await command.ExecuteScalarAsync();

            entity.Id = Convert.ToInt32(result);

            return entity.Id;
        }

        public async Task AddManyAsync(IEnumerable<Event> entities)
        {
            await using NpgsqlConnection connection = await _service.GetConnectionAsync();

            StringBuilder queryBuilder = new("INSERT INTO Events (Name, EventTime, Sex, Round, StartListurl, ResultsUrl, SummaryUrl, PointsUrl) VALUES ");
            List<NpgsqlParameter> parameters = new();
            int index = 0;

            foreach (var entity in entities)
            {
                queryBuilder.Append($"(@Name{index}, @EventTime{index}, @Sex{index}, @Round{index}, @StartListurl{index}, @ResultsUrl{index}, @SummaryUrl{index}, @PointsUrl{index}),");
                parameters.Add(new NpgsqlParameter($"@Name{index}", entity.Name ?? (object)DBNull.Value));
                parameters.Add(new NpgsqlParameter($"@EventTime{index}", entity.EventTime ?? (object)DBNull.Value));
                parameters.Add(new NpgsqlParameter($"@Sex{index}", entity.Sex ?? (object)DBNull.Value));
                parameters.Add(new NpgsqlParameter($"@Round{index}", entity.Round ?? (object)DBNull.Value));
                parameters.Add(new NpgsqlParameter($"@StartListUrl{index}", entity.StartListUrl ?? (object)DBNull.Value));
                parameters.Add(new NpgsqlParameter($"@ResultsUrl{index}", entity.ResultsUrl ?? (object)DBNull.Value));
                parameters.Add(new NpgsqlParameter($"@SummaryUrl{index}", entity.SummaryUrl ?? (object)DBNull.Value));
                parameters.Add(new NpgsqlParameter($"@PointsUrl{index}", entity.PointsUrl ?? (object)DBNull.Value));
                index++;
            }

            queryBuilder.Length--; // Remove last comma
            queryBuilder.Append(";");

            await using NpgsqlCommand command = new(queryBuilder.ToString(), connection);
            command.Parameters.AddRange(parameters.ToArray());
            await command.ExecuteNonQueryAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            NpgsqlConnection connection = await _service.GetConnectionAsync();

            string query = "DELETE FROM Events WHERE Id = @Id";

            await using NpgsqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@Id", id);

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<IEnumerable<Event>> GetAllAsync()
        {
            await using NpgsqlConnection connection = await _service.GetConnectionAsync();

            string query = "SELECT Id, Name, EventTime, Sex, Round, StartListurl, ResultsUrl, SummaryUrl, PointsUrl FROM Events";

            await using NpgsqlCommand command = new(query, connection);

            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

            List<Event> events = new();

            while (await reader.ReadAsync())
            {
                events.Add(new Event
                {
                    Id = reader.GetInt32(0),
                    Name = reader.IsDBNull(1) ? null : reader.GetString(1),
                    EventTime = reader.IsDBNull(2) ? null : reader.GetTimeSpan(2),
                    Sex = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Round = reader.IsDBNull(4) ? null : reader.GetString(4),
                    StartListUrl = reader.IsDBNull(5) ? null : reader.GetString(5),
                    ResultsUrl = reader.IsDBNull(6) ? null : reader.GetString(6),
                    SummaryUrl = reader.IsDBNull(7) ? null : reader.GetString(7),
                    PointsUrl = reader.IsDBNull(8) ? null : reader.GetString(8)
                });
            }

            return events;
        }

        public async Task<Event?> GetByIdAsync(int id)
        {
            await using NpgsqlConnection connection = await _service.GetConnectionAsync();

            string query = "SELECT Id, Name, EventTime, Sex, Round, StartListurl, ResultsUrl, SummaryUrl, PointsUrl FROM Events WHERE Id = @Id";

            await using NpgsqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Event
                {
                    Id = reader.GetInt32(0),
                    Name = reader.IsDBNull(1) ? null : reader.GetString(1),
                    EventTime = reader.IsDBNull(2) ? null : reader.GetTimeSpan(2),
                    Sex = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Round = reader.IsDBNull(4) ? null : reader.GetString(4),
                    StartListUrl = reader.IsDBNull(5) ? null : reader.GetString(5),
                    ResultsUrl = reader.IsDBNull(6) ? null : reader.GetString(6),
                    SummaryUrl = reader.IsDBNull(7) ? null : reader.GetString(7),
                    PointsUrl = reader.IsDBNull(8) ? null : reader.GetString(8)
                };
            }

            return null;
        }

        public async Task<bool> UpdateAsync(Event entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            await using NpgsqlConnection connection = await _service.GetConnectionAsync();

            string query =  "UPDATE Events SET Name = @Name, EventTime = @EventTime, Sex = @Sex, Round = @Round, " +
                            "StartListurl = @StartListurl, ResultsUrl = @ResultsUrl, SummaryUrl = @SummaryUrl, PointsUrl = @PointsUrl WHERE Id = @Id";

            await using NpgsqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@Id", entity.Id);
            command.Parameters.AddWithValue("@Name", entity.Name ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@EventTime", entity.EventTime ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Sex", entity.Sex ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Round", entity.Round ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@StartListUrl", entity.StartListUrl ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ResultsUrl", entity.ResultsUrl ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@SummaryUrl", entity.SummaryUrl ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@PointsUrl", entity.PointsUrl ?? (object)DBNull.Value);

            return await command.ExecuteNonQueryAsync() > 0;
        }
    }
}
