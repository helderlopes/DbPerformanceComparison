using Npgsql;

namespace DbPerformanceComparison.Infrastructure.Postgres
{
    public class PostgresService
    {
        private readonly string _connectionString;

        public PostgresService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<NpgsqlConnection> GetConnectionAsync()
        {
            NpgsqlConnection conn = new(_connectionString);
            await conn.OpenAsync();
            return conn;
        }
    }
}
