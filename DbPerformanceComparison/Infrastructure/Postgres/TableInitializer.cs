using Npgsql;

namespace DbPerformanceComparison.Infrastructure.Postgres
{
    public class TableInitializer
    {
        private readonly PostgresService _postgresService;

        public TableInitializer(PostgresService postgresService)
        {
            _postgresService = postgresService;
        }

        public async Task InitializeTablesAsync()
        {
            NpgsqlConnection connection = await _postgresService.GetConnectionAsync();

            NpgsqlCommand command = connection.CreateCommand();

            command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Athletes (
                        Id SERIAL PRIMARY KEY,
                        Name TEXT,
                        Sex TEXT,
                        Country TEXT
                    );

                    CREATE TABLE IF NOT EXISTS Events (
                        Id SERIAL PRIMARY KEY,
                        Name TEXT,
                        LocalTime TIME,
                        Sex TEXT,
                        Round TEXT,
                        StartListUrl TEXT,
                        ResultsUrl TEXT,
                        SummaryUrl TEXT,
                        PointsUrl TEXT
                    );

                    CREATE TABLE IF NOT EXISTS Results (
                        Id SERIAL PRIMARY KEY,
                        AthleteId INTEGER REFERENCES Athletes(Id) ON DELETE CASCADE,
                        EventId INTEGER REFERENCES Events(Id) ON DELETE CASCADE,
                        Position INTEGER,
                        Bib INTEGER,
                        Mark TIME
                    );
                ";

            await command.ExecuteNonQueryAsync();
        }
    }
}
