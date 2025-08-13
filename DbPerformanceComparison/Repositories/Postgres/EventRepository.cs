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
        public string DatabaseName => "PostgreSQL";
        private readonly PostgresService _service;

        public EventRepository(PostgresService service)
        {
            _service = service;
        }

        public async Task AddAsync(Event entity)
        {
            NpgsqlConnection connection = await _service.GetConnectionAsync();
            string query =  "INSERT INTO Events (Id, Name, EventTime, Sex, Round, StartListUrl, ResultsUrl, SummaryUrl, PointsUrl) " +
                            "VALUES (@Id, @Name, @EventTime, @Sex, @Round, @StartListUrl, @ResultsUrl, @SummaryUrl, @PointsUrl) RETURNING Id";

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

            await command.ExecuteScalarAsync();
        }

        public async Task AddManyAsync(IEnumerable<Event> entities)
        {
            const int maxParameters = 65535;
            const int paramsPerRow = 9; // Id, Name, EventTime, Sex, Round, StartListUrl, ResultsUrl, SummaryUrl, PointsUrl
            int maxRowsPerBatch = maxParameters / paramsPerRow;

            var entityList = entities.ToList();
            int total = entityList.Count;

            await using var connection = await _service.GetConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                for (int i = 0; i < total; i += maxRowsPerBatch)
                {
                    int currentBatchSize = Math.Min(maxRowsPerBatch, total - i);

                    StringBuilder queryBuilder = new("INSERT INTO Events (Id, Name, EventTime, Sex, Round, StartListUrl, ResultsUrl, SummaryUrl, PointsUrl) VALUES ");
                    List<NpgsqlParameter> parameters = new(currentBatchSize * paramsPerRow);

                    for (int j = 0; j < currentBatchSize; j++)
                    {
                        var entity = entityList[i + j];
                        queryBuilder.Append($"(@Id{j}, @Name{j}, @EventTime{j}, @Sex{j}, @Round{j}, @StartListUrl{j}, @ResultsUrl{j}, @SummaryUrl{j}, @PointsUrl{j}),");
                        parameters.Add(new NpgsqlParameter($"@Id{j}", entity.Id));
                        parameters.Add(new NpgsqlParameter($"@Name{j}", entity.Name ?? (object)DBNull.Value));
                        parameters.Add(new NpgsqlParameter($"@EventTime{j}", entity.EventTime ?? (object)DBNull.Value));
                        parameters.Add(new NpgsqlParameter($"@Sex{j}", entity.Sex ?? (object)DBNull.Value));
                        parameters.Add(new NpgsqlParameter($"@Round{j}", entity.Round ?? (object)DBNull.Value));
                        parameters.Add(new NpgsqlParameter($"@StartListUrl{j}", entity.StartListUrl ?? (object)DBNull.Value));
                        parameters.Add(new NpgsqlParameter($"@ResultsUrl{j}", entity.ResultsUrl ?? (object)DBNull.Value));
                        parameters.Add(new NpgsqlParameter($"@SummaryUrl{j}", entity.SummaryUrl ?? (object)DBNull.Value));
                        parameters.Add(new NpgsqlParameter($"@PointsUrl{j}", entity.PointsUrl ?? (object)DBNull.Value));
                    }

                    queryBuilder.Length--;
                    queryBuilder.Append(';');

                    await using var command = new NpgsqlCommand(queryBuilder.ToString(), connection, transaction);
                    command.Parameters.AddRange(parameters.ToArray());
                    await command.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
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

            string query = "SELECT Id, Name, EventTime, Sex, Round, StartListUrl, ResultsUrl, SummaryUrl, PointsUrl FROM Events";

            await using NpgsqlCommand command = new(query, connection);

            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

            List<Event> events = new();

            while (await reader.ReadAsync())
            {
                events.Add(new Event
                {
                    Id = reader.GetGuid(0),
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

        public async Task<Event?> GetByIdAsync(Guid id)
        {
            await using NpgsqlConnection connection = await _service.GetConnectionAsync();

            string query = "SELECT Name, EventTime, Sex, Round, StartListUrl, ResultsUrl, SummaryUrl, PointsUrl FROM Events WHERE Id = @Id";

            await using NpgsqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Event
                {
                    Id = id,
                    Name = reader.IsDBNull(0) ? null : reader.GetString(0),
                    EventTime = reader.IsDBNull(1) ? null : reader.GetTimeSpan(1),
                    Sex = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Round = reader.IsDBNull(3) ? null : reader.GetString(3),
                    StartListUrl = reader.IsDBNull(4) ? null : reader.GetString(4),
                    ResultsUrl = reader.IsDBNull(5) ? null : reader.GetString(5),
                    SummaryUrl = reader.IsDBNull(6) ? null : reader.GetString(6),
                    PointsUrl = reader.IsDBNull(7) ? null : reader.GetString(7)
                };
            }

            return null;
        }

        public async Task<bool> UpdateAsync(Event entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            await using NpgsqlConnection connection = await _service.GetConnectionAsync();

            string query =  "UPDATE Events SET Name = @Name, EventTime = @EventTime, Sex = @Sex, Round = @Round, " +
                            "StartListUrl = @StartListUrl, ResultsUrl = @ResultsUrl, SummaryUrl = @SummaryUrl, PointsUrl = @PointsUrl WHERE Id = @Id";

            await using NpgsqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@Name", entity.Name ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@EventTime", entity.EventTime ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Sex", entity.Sex ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Round", entity.Round ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@StartListUrl", entity.StartListUrl ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ResultsUrl", entity.ResultsUrl ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@SummaryUrl", entity.SummaryUrl ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@PointsUrl", entity.PointsUrl ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Id", entity.Id);

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<int> DeleteAllAsync()
        {
            await using NpgsqlConnection connection = await _service.GetConnectionAsync();

            string query = "DELETE FROM Events";

            await using NpgsqlCommand command = new(query, connection);

            return await command.ExecuteNonQueryAsync();
        }
    }
}
