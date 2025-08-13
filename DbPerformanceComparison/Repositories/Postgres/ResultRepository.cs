using DbPerformanceComparison.Infrastructure.Postgres;
using DbPerformanceComparison.Repositories.Interfaces;
using Npgsql;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbPerformanceComparison.Repositories.Postgres
{
    public class ResultRepository : IRepository<Result>
    {
        public string DatabaseName => "PostgreSQL";
        private readonly PostgresService _service;

        public ResultRepository(PostgresService service)
        {
            _service = service;
        }

        public async Task AddAsync(Result entity)
        {
            NpgsqlConnection connection = await _service.GetConnectionAsync();
            string query =  "INSERT INTO Results (Id, AthleteId, EventId, Position, Bib, Mark) " +
                            "VALUES (@Id, @AthleteId, @EventId, @Position, @Bib, @Mark)";

            await using NpgsqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@Id", entity.Id);
            command.Parameters.AddWithValue("@AthleteId", entity.AthleteId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@EventId", entity.EventId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Position", entity.Position ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Bib", entity.Bib ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Mark", entity.Mark ?? (object)DBNull.Value);

            try
            {
                await command.ExecuteScalarAsync();
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.ForeignKeyViolation)
            {
                Console.WriteLine("Error: invalid Foreign Key.");
            }
        }

        public async Task AddManyAsync(IEnumerable<Result> entities)
        {
            const int maxParameters = 65535;
            const int paramsPerRow = 6; // Id, AthleteId, EventId, Position, Bib, Mark
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

                    StringBuilder queryBuilder = new("INSERT INTO Results (Id, AthleteId, EventId, Position, Bib, Mark) VALUES ");
                    List<NpgsqlParameter> parameters = new(currentBatchSize * paramsPerRow);

                    for (int j = 0; j < currentBatchSize; j++)
                    {
                        var entity = entityList[i + j];
                        queryBuilder.Append($"(@Id{j}, @AthleteId{j}, @EventId{j}, @Position{j}, @Bib{j}, @Mark{j}),");

                        parameters.Add(new NpgsqlParameter($"@Id{j}", entity.Id));
                        parameters.Add(new NpgsqlParameter($"@AthleteId{j}", entity.AthleteId ?? (object)DBNull.Value));
                        parameters.Add(new NpgsqlParameter($"@EventId{j}", entity.EventId ?? (object)DBNull.Value));
                        parameters.Add(new NpgsqlParameter($"@Position{j}", entity.Position ?? (object)DBNull.Value));
                        parameters.Add(new NpgsqlParameter($"@Bib{j}", entity.Bib ?? (object)DBNull.Value));
                        parameters.Add(new NpgsqlParameter($"@Mark{j}", entity.Mark ?? (object)DBNull.Value));
                    }

                    queryBuilder.Length--;
                    queryBuilder.Append(';');

                    await using var command = new NpgsqlCommand(queryBuilder.ToString(), connection, transaction);
                    command.Parameters.AddRange(parameters.ToArray());
                    await command.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.ForeignKeyViolation)
            {
                Console.WriteLine("Error: invalid Foreign Key.");
                await transaction.RollbackAsync();
                throw;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            await using NpgsqlConnection connection = await _service.GetConnectionAsync();

            string query = "DELETE FROM Results WHERE Id = @Id";

            await using NpgsqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@Id", id);

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<IEnumerable<Result>> GetAllAsync()
        {
            await using NpgsqlConnection connection = await _service.GetConnectionAsync();

            string query = "SELECT Id, AthleteId, EventId, Position, Bib, Mark FROM Results";

            await using NpgsqlCommand command = new(query, connection);

            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

            List<Result> results = new();

            while (await reader.ReadAsync())
            {
                results.Add(new Result
                {
                    Id = reader.GetGuid(0),
                    AthleteId = reader.IsDBNull(1) ? null : reader.GetGuid(1),
                    EventId = reader.IsDBNull(2) ? null : reader.GetGuid(2),
                    Position = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    Bib = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                    Mark = reader.IsDBNull(5) ? null : reader.GetTimeSpan(5)
                });
            }

            return results;
        }

        public async Task<Result?> GetByIdAsync(Guid id)
        {
            await using NpgsqlConnection connection = await _service.GetConnectionAsync();

            string query = "SELECT AthleteId, EventId, Position, Bib, Mark FROM Results WHERE Id = @Id";

            await using NpgsqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Result
                {
                    Id = id,
                    AthleteId = reader.IsDBNull(0) ? null : reader.GetGuid(0),
                    EventId = reader.IsDBNull(1) ? null : reader.GetGuid(1),
                    Position = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                    Bib = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    Mark = reader.IsDBNull(4) ? null : reader.GetTimeSpan(4)
                };
            }

            return null;
        }

        public async Task<bool> UpdateAsync(Result entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            await using NpgsqlConnection connection = await _service.GetConnectionAsync();

            string query = "UPDATE Results SET AthleteId = @AthleteId, EventId = @EventId, Position = @Position, Bib = @Bib, Mark = @Mark WHERE Id = @Id";

            await using NpgsqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@Id", entity.Id);
            command.Parameters.AddWithValue("@AthleteId", entity.AthleteId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@EventId", entity.EventId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Position", entity.Position ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Bib", entity.Bib ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Mark", entity.Mark ?? (object)DBNull.Value);

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<int> DeleteAllAsync()
        {
            await using NpgsqlConnection connection = await _service.GetConnectionAsync();

            string query = "DELETE FROM Results";

            await using NpgsqlCommand command = new(query, connection);

            return await command.ExecuteNonQueryAsync();
        }
    }
}
