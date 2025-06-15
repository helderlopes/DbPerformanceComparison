using DbPerformanceComparison.Infrastructure.Mongo;
using DbPerformanceComparison.Infrastructure.Postgres;
using DbPerformanceComparison.Monitoring;
using DbPerformanceComparison.Repositories.Interfaces;
using DbPerformanceComparison.Repositories.Mongo;
using DbPerformanceComparison.Repositories.Postgres;
using DbPerformanceComparison.Services.ConfigurationBuilder;
using DbPerformanceComparison.Services.Parser;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace DbPerformanceComparison
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            BenchmarkRunner benchmarkRunner = new();
            await benchmarkRunner.Init();
            await benchmarkRunner.RunTestsAsync();
        }
    }
}
