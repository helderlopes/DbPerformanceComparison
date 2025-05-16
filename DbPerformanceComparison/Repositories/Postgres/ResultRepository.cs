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
    public class ResultRepository : IRepository<Result, int>
    {
        private readonly PostgresService _service;

        public ResultRepository(PostgresService service)
        {
            _service = service;
        }

        public async Task AddAsync(Result entity)
        {
            NpgsqlConnection connection = await _service.GetConnectionAsync();
            string query =  "INSERT INTO Results (AthleteId, EventId, Position, Bib, Mark) " +
                            "VALUES (@AthleteId, @EventId, @Position, @Bib, @Mark) RETURNING Id";

            await using NpgsqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@AthleteId", entity.AthleteId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@EventId", entity.EventId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Position", entity.Position ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Bib", entity.Bib ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Mark", entity.Mark ?? (object)DBNull.Value);

            entity.Id = -1;

            try
            {
                entity.Id = Convert.ToInt32(await command.ExecuteScalarAsync());
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.ForeignKeyViolation)
            {
                Console.WriteLine("Error: invalid Foreign Key.");
            }
        }

        public async Task AddManyAsync(IEnumerable<Result> entities)
        {
            await using NpgsqlConnection connection = await _service.GetConnectionAsync();

            StringBuilder queryBuilder = new("INSERT INTO Results (AthleteId, EventId, Position, Bib, Mark) VALUES ");
            List<NpgsqlParameter> parameters = new();
            int index = 0;

            foreach (var entity in entities)
            {
                queryBuilder.Append($"(@AthleteId{index}, @EventId{index}, @Position{index}, @Bib{index}, @Mark{index}),");
                parameters.Add(new NpgsqlParameter($"@AthleteId{index}", entity.AthleteId ?? (object)DBNull.Value));
                parameters.Add(new NpgsqlParameter($"@EventId{index}", entity.EventId ?? (object)DBNull.Value));
                parameters.Add(new NpgsqlParameter($"@Position{index}", entity.Position ?? (object)DBNull.Value));
                parameters.Add(new NpgsqlParameter($"@Bib{index}", entity.Bib ?? (object)DBNull.Value));
                parameters.Add(new NpgsqlParameter($"@Mark{index}", entity.Mark ?? (object)DBNull.Value));
                index++;
            }

            queryBuilder.Length--; // Remove last comma
            queryBuilder.Append(";");

            await using NpgsqlCommand command = new(queryBuilder.ToString(), connection);
            command.Parameters.AddRange(parameters.ToArray());

            try
            {
                await command.ExecuteNonQueryAsync();
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.ForeignKeyViolation)
            {
                Console.WriteLine("Error: invalid Foreign Key.");
            }
        }

        public async Task<bool> DeleteAsync(int id)
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
                    Id = reader.GetInt32(0),
                    AthleteId = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                    EventId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                    Position = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    Bib = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                    Mark = reader.IsDBNull(5) ? null : reader.GetTimeSpan(5)
                });
            }

            return results;
        }

        public async Task<Result?> GetByIdAsync(int id)
        {
            await using NpgsqlConnection connection = await _service.GetConnectionAsync();

            string query = "SELECT Id, AthleteId, EventId, Position, Bib, Mark FROM Results WHERE Id = @Id";

            await using NpgsqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Result
                {
                    Id = id,
                    AthleteId = reader.IsDBNull(0) ? null : reader.GetInt32(0),
                    EventId = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                    Position = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                    Bib = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    Mark = reader.IsDBNull(4) ? null : reader.GetTimeSpan(4)
                };
            }

            return null;
        }

        public async Task<bool> UpdateAsync(Result entity, int id)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            await using NpgsqlConnection connection = await _service.GetConnectionAsync();

            string query = "UPDATE Results SET AthleteId = @AthleteId, EventId = @EventId, Position = @Position, Bib = @Bib, Mark = @Mark WHERE Id = @Id";

            await using NpgsqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@Id", id);
            command.Parameters.AddWithValue("@AthleteId", entity.AthleteId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@EventId", entity.EventId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Position", entity.Position ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Bib", entity.Bib ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Mark", entity.Mark ?? (object)DBNull.Value);

            return await command.ExecuteNonQueryAsync() > 0;
        }
    }
}
