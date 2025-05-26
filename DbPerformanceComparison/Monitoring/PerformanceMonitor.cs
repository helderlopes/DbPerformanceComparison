using DbPerformanceComparison.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbPerformanceComparison.Monitoring
{
    public class PerformanceMonitor
    {
        private static readonly MetricLogger MetricLogger = new(true);

        public async Task<T> MeasureAddAsync<T>(
           IRepository<T> repository,
           T entity,
           Func<T, Task<T>> action) where T : class
        {
            var stopwatch = Stopwatch.StartNew();

            T result = await action(entity);

            stopwatch.Stop();

            var metric = new MetricResult
            {
                Operation = action.Method.Name,
                Database = repository.DatabaseName,
                EntityType = typeof(T).Name,
                EntityCount = 1,
                ElapsedUs = (long)stopwatch.Elapsed.TotalMicroseconds
            };

            MetricLogger.Log(metric);
            return result;
        }

        public async Task<IEnumerable<T>> MeasureAddManyAsync<T>(
           IRepository<T> repository,
           IEnumerable<T> entities,
           Func<IEnumerable<T>, Task<IEnumerable<T>>> action) where T : class
        {
            var stopwatch = Stopwatch.StartNew();

            IEnumerable<T> result = await action(entities);

            stopwatch.Stop();

            var metric = new MetricResult
            {
                Operation = action.Method.Name,
                Database = repository.DatabaseName,
                EntityType = typeof(T).Name,
                EntityCount = entities.Count(),
                ElapsedUs = (long)stopwatch.Elapsed.TotalMicroseconds
            };

            MetricLogger.Log(metric);
            return result;
        }

        public async Task<T?> MeasureGetByIdAsync<T>(
           IRepository<T> repository,
           Guid id,
           Func<Guid, Task<T?>> action) where T : class
        {
            var stopwatch = Stopwatch.StartNew();

            T? result = await action(id);

            stopwatch.Stop();

            var metric = new MetricResult
            {
                Operation = action.Method.Name,
                Database = repository.DatabaseName,
                EntityType = typeof(T).Name,
                EntityCount = 1,
                ElapsedUs = (long)stopwatch.Elapsed.TotalMicroseconds
            };

            MetricLogger.Log(metric);
            return result;
        }
    }
}
