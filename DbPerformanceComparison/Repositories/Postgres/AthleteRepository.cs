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
            const int maxParameters = 65535;
            const int paramsPerRow = 4; // Id, Name, Sex, Country
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

                    StringBuilder queryBuilder = new("INSERT INTO Athletes (Id, Name, Sex, Country) VALUES ");
                    List<NpgsqlParameter> parameters = new(currentBatchSize * paramsPerRow);

                    for (int j = 0; j < currentBatchSize; j++)
                    {
                        var entity = entityList[i + j];
                        queryBuilder.Append($"(@Id{j}, @Name{j}, @Sex{j}, @Country{j}),");
                        parameters.Add(new NpgsqlParameter($"@Id{j}", entity.Id));
                        parameters.Add(new NpgsqlParameter($"@Name{j}", entity.Name ?? (object)DBNull.Value));
                        parameters.Add(new NpgsqlParameter($"@Sex{j}", entity.Sex ?? (object)DBNull.Value));
                        parameters.Add(new NpgsqlParameter($"@Country{j}", entity.Country ?? (object)DBNull.Value));
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

            string query = "SELECT Name, Sex, Country FROM Athletes WHERE Id = @Id";

            await using NpgsqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Athlete
                {
                    Id = id,
                    Name = reader.IsDBNull(0) ? null : reader.GetString(0),
                    Sex = reader.IsDBNull(1) ? null : reader.GetString(1),
                    Country = reader.IsDBNull(2) ? null : reader.GetString(2)
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

        public async Task<int> DeleteAllAsync()
        {
            await using NpgsqlConnection connection = await _service.GetConnectionAsync();

            string query = "DELETE FROM Athletes";

            await using NpgsqlCommand command = new(query, connection);

            return await command.ExecuteNonQueryAsync();
        }
    }
}
