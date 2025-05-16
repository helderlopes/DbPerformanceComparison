using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace DbPerformanceComparison.Services.ConfigurationBuilder
{
    public class ConfigurationBuilderService
    {
        IConfigurationRoot _configurationRoot;
        public ConfigurationBuilderService()
        {
            _configurationRoot = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.json", optional: true)
                .Build();
        }

        public string GetPostgresConnectionString()
        {
            return $"Host={_configurationRoot["Postgres:Host"]};Port={_configurationRoot["Postgres:Port"]};Username={_configurationRoot["Postgres:Username"]};Password={_configurationRoot["Postgres:Password"]};Database={_configurationRoot["Postgres:Database"]}";
        }

        public string GetMongoConnectionString()
        {
            return $"mongodb://{_configurationRoot["Mongo:Username"]}:{_configurationRoot["Mongo:Password"]}@{_configurationRoot["Mongo:Host"]}:{_configurationRoot["Mongo:Port"]}";
        }

        public string GetMongoDatabaseName()
        {
            return _configurationRoot["Mongo:Database"] ?? string.Empty;
        }
    }
}
