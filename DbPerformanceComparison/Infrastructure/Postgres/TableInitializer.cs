using Npgsql;
using System.Linq;
using System.Text;

namespace DbPerformanceComparison.Infrastructure.Postgres
{
    public class TableInitializer
    {
        private readonly PostgresService _postgresService;

        public TableInitializer(PostgresService postgresService)
        {
            _postgresService = postgresService;
        }

        public async Task InitializeTablesAsync(bool dropTables = false)
        {
            NpgsqlConnection connection = await _postgresService.GetConnectionAsync();

            NpgsqlCommand command = connection.CreateCommand();

            StringBuilder commandText = new();

            if (dropTables)
            {
                commandText.Append(@"
                        DROP TABLE IF EXISTS Results; 
                        DROP TABLE IF EXISTS Events; 
                        DROP TABLE IF EXISTS Athletes;");
            }

            commandText.Append(@"
                       CREATE TABLE IF NOT EXISTS Athletes (
                           Id UUID PRIMARY KEY,
                           Name TEXT,
                           Sex TEXT,
                           Country TEXT
                       );

                       CREATE TABLE IF NOT EXISTS Events (
                           Id UUID PRIMARY KEY,
                           Name TEXT,
                           EventTime TIME,
                           Sex TEXT,
                           Round TEXT,
                           StartListUrl TEXT,
                           ResultsUrl TEXT,
                           SummaryUrl TEXT,
                           PointsUrl TEXT
                       );

                       CREATE TABLE IF NOT EXISTS Results (
                           Id UUID PRIMARY KEY,
                           AthleteId INTEGER REFERENCES Athletes(Id) ON DELETE CASCADE,
                           EventId INTEGER REFERENCES Events(Id) ON DELETE CASCADE,
                           Position INTEGER,
                           Bib INTEGER,
                           Mark TIME
                       );
                   ");

            command.CommandText = commandText.ToString(); 
            await command.ExecuteNonQueryAsync();
        }
    }
}
