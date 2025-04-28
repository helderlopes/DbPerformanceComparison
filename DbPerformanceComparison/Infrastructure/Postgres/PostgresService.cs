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

        public NpgsqlConnection GetConnection()
        {
            var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            return conn;
        }
    }
}
