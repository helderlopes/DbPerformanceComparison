using DbPerformanceComparison.Infrastructure.Postgres;
using DbPerformanceComparison.Repositories.Interfaces;
using Npgsql;
using System.Text;

namespace DbPerformanceComparison.Repositories.Postgres
{
    public class AthleteRepository : IRepository<Athlete>
    {
        public string DatabaseName => "PostgreSQL";
        private readonly PostgresService _service;

        public AthleteRepository(PostgresService service)
        {
            _service = service;
        }

        public async Task AddAsync(Athlete entity)
        {
            NpgsqlConnection connection = await _service.GetConnectionAsync();
            string query = "INSERT INTO Athletes (Id, Name, Sex, Country) VALUES (@Id, @Name, @Sex, @Country)";

            await using NpgsqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@Id", entity.Id);
            command.Parameters.AddWithValue("@Name", entity.Name ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Sex", entity.Sex ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Country", entity.Country ?? (object)DBNull.Value);

            await command.ExecuteNonQueryAsync();
        }

        public async Task AddManyAsync(IEnumerable<Athlete> entities)
        {
            await using NpgsqlConnection connection = await _service.GetConnectionAsync();

            StringBuilder queryBuilder = new("INSERT INTO Athletes (Id, Name, Sex, Country) VALUES ");
            List<NpgsqlParameter> parameters = new();
            int index = 0;

            foreach (var entity in entities)
            {
                queryBuilder.Append($"(@Id{index}, @Name{index}, @Sex{index}, @Country{index}),");
                parameters.Add(new NpgsqlParameter($"@Id{index}", entity.Id));
                parameters.Add(new NpgsqlParameter($"@Name{index}", entity.Name ?? (object)DBNull.Value));
                parameters.Add(new NpgsqlParameter($"@Sex{index}", entity.Sex ?? (object)DBNull.Value));
                parameters.Add(new NpgsqlParameter($"@Country{index}", entity.Country ?? (object)DBNull.Value));
                index++;
            }

            queryBuilder.Length--; // Remove last comma
            queryBuilder.Append(";");

            await using NpgsqlCommand command = new(queryBuilder.ToString(), connection);
            command.Parameters.AddRange(parameters.ToArray());
            await command.ExecuteNonQueryAsync();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            await using NpgsqlConnection connection = await _service.GetConnectionAsync();

            string query = "DELETE FROM Athletes WHERE Id = @Id";

            await using NpgsqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@Id", id);

            return await command.ExecuteNonQueryAsync() > 0;
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
                    Id = reader.GetGuid(0),
                    Name = reader.IsDBNull(1) ? null : reader.GetString(1),
                    Sex = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Country = reader.IsDBNull(3) ? null : reader.GetString(3)
                });
            }

            return athletes;
        }

        public async Task<Athlete?> GetByIdAsync(Guid id)
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
                    Id = reader.GetGuid(0),
                    Name = reader.IsDBNull(1) ? null : reader.GetString(1),
                    Sex = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Country = reader.IsDBNull(3) ? null : reader.GetString(3)
                };
            }

            return null;
        }

        public async Task<bool> UpdateAsync(Athlete entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            await using NpgsqlConnection connection = await _service.GetConnectionAsync();

            string query = "UPDATE Athletes SET Name = @Name, Sex = @Sex, Country = @Country WHERE Id = @Id";

            await using NpgsqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@Name", entity.Name ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Sex", entity.Sex ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Country", entity.Country ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Id", entity.Id);

            return await command.ExecuteNonQueryAsync() > 0;
        }
    }
}
