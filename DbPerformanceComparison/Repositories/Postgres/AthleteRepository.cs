﻿using DbPerformanceComparison.Infrastructure.Postgres;
using DbPerformanceComparison.Repositories.Interfaces;
using Npgsql;
using System.Text;

namespace DbPerformanceComparison.Repositories.Postgres
{
    public class AthleteRepository : IRepository<Athlete>
    {
        private readonly PostgresService _service;

        public AthleteRepository(PostgresService service)
        {
            _service = service;
        }
        public async Task AddAsync(Athlete entity)
        {
            NpgsqlConnection connection = await _service.GetConnectionAsync();
            string query = "INSERT INTO Athletes (Name, Sex, Country) VALUES (@Name, @Sex, @Country)";

            await using NpgsqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@Name", entity.Name ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Sex", entity.Sex ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Country", entity.Country ?? (object)DBNull.Value);

            await command.ExecuteNonQueryAsync();
        }

        public async Task AddManyAsync(IEnumerable<Athlete> entities)
        {
            await using NpgsqlConnection connection = await _service.GetConnectionAsync();

            StringBuilder queryBuilder = new("INSERT INTO Athletes (Name, Sex, Country) VALUES ");
            List<NpgsqlParameter> parameters = new();
            int index = 0;

            foreach (var athlete in entities)
            {
                queryBuilder.Append($"(@Name{index}, @Sex{index}, @Country{index}),");
                parameters.Add(new NpgsqlParameter($"@Name{index}", athlete.Name ?? (object)DBNull.Value));
                parameters.Add(new NpgsqlParameter($"@Sex{index}", athlete.Sex ?? (object)DBNull.Value));
                parameters.Add(new NpgsqlParameter($"@Country{index}", athlete.Country ?? (object)DBNull.Value));
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
            await using NpgsqlConnection connection = await _service.GetConnectionAsync();

            string query = "DELETE FROM Athletes WHERE Id = @Id";

            await using NpgsqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@Id", id);

            int rowsAffected = await command.ExecuteNonQueryAsync();

            return rowsAffected > 0;
        }

        public async Task<IEnumerable<Athlete>> GetAllAsync()
        {
            await using NpgsqlConnection connection = await _service.GetConnectionAsync();

            string query = "SELECT Id, Name, Sex, Country FROM Athletes";

            await using NpgsqlCommand command = new(query, connection);

            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

            List<Athlete> athletes = new();

            while (await reader.ReadAsync())
            {
                athletes.Add(new Athlete
                {
                    Id = reader.GetInt32(0),
                    Name = reader.IsDBNull(1) ? null : reader.GetString(1),
                    Sex = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Country = reader.IsDBNull(3) ? null : reader.GetString(3)
                });
            }

            return athletes;
        }

        public async Task<Athlete?> GetByIdAsync(int id)
        {
            await using NpgsqlConnection connection = await _service.GetConnectionAsync();

            string query = "SELECT Id, Name, Sex, Country FROM Athletes WHERE Id = @Id";

            await using NpgsqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Athlete
                {
                    Id = reader.GetInt32(0),
                    Name = reader.IsDBNull(1) ? null : reader.GetString(1),
                    Sex = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Country = reader.IsDBNull(3) ? null : reader.GetString(3)
                };
            }

            return null;
        }

        public async Task UpdateAsync(Athlete entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            await using NpgsqlConnection connection = await _service.GetConnectionAsync();

            string query = "UPDATE Athletes SET Name = @Name, Sex = @Sex, Country = @Country WHERE Id = @Id";

            await using NpgsqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@Id", entity.Id);
            command.Parameters.AddWithValue("@Name", entity.Name ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Sex", entity.Sex ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Country", entity.Country ?? (object)DBNull.Value);

            await command.ExecuteNonQueryAsync();
        }
    }
}
