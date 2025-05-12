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
        private readonly PostgresService _service;

        public ResultRepository(PostgresService service)
        {
            _service = service;
        }

        /*
            Id SERIAL PRIMARY KEY,
            AthleteId INTEGER REFERENCES Athletes(Id) ON DELETE CASCADE,
            EventId INTEGER REFERENCES Events(Id) ON DELETE CASCADE,
            Position INTEGER,
            Bib INTEGER,
            Mark TIME
         */
        public async Task<int> AddAsync(Result entity)
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
                var result = await command.ExecuteScalarAsync();

                entity.Id = Convert.ToInt32(result);
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.ForeignKeyViolation)
            {
                Console.WriteLine("Error: invalid Foreign Key.");
            }

            return entity.Id;
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

        public Task<bool> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Result>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Result?> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateAsync(Result entity)
        {
            throw new NotImplementedException();
        }
    }
}
