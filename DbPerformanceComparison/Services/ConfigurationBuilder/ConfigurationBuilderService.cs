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
    }
}
