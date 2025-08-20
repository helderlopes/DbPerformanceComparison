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
        private static readonly MetricLogger MetricLogger = new();
        private int _scale;

        public PerformanceMonitor(int scale = 1)
        {
            _scale = scale;
        }

        public async Task MeasureAddAsync<T>(
           IRepository<T> repository,
           T entity,
           Func<T, Task> action) where T : class
        {
            var stopwatch = Stopwatch.StartNew();

            await action(entity);

            stopwatch.Stop();

            var metric = new MetricResult
            {
                Operation = action.Method.Name,
                Database = repository.DatabaseName,
                EntityType = typeof(T).Name,
                EntityCount = 1,
                ElapsedUs = (long)stopwatch.Elapsed.TotalMicroseconds,
                Scale = _scale
            };

            MetricLogger.Log(metric);
        }

        public async Task MeasureAddManyAsync<T>(
           IRepository<T> repository,
           IEnumerable<T> entities,
           Func<IEnumerable<T>, Task> action) where T : class
        {
            var stopwatch = Stopwatch.StartNew();

            await action(entities);

            stopwatch.Stop();

            var metric = new MetricResult
            {
                Operation = action.Method.Name,
                Database = repository.DatabaseName,
                EntityType = typeof(T).Name,
                EntityCount = entities.Count(),
                ElapsedUs = (long)stopwatch.Elapsed.TotalMicroseconds,
                Scale = _scale
            };

            MetricLogger.Log(metric);
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
                ElapsedUs = (long)stopwatch.Elapsed.TotalMicroseconds,
                Scale = _scale
            };

            MetricLogger.Log(metric);
            return result ?? default;
        }

        public async Task<IEnumerable<T>> MeasureGetAllAsync<T>(
           IRepository<T> repository,
           Func<Task<IEnumerable<T>>> action) where T : class
        {
            var stopwatch = Stopwatch.StartNew();

            IEnumerable<T> result = await action();

            stopwatch.Stop();

            var metric = new MetricResult
            {
                Operation = action.Method.Name,
                Database = repository.DatabaseName,
                EntityType = typeof(T).Name,
                EntityCount = result?.Count() ?? 0,
                ElapsedUs = (long)stopwatch.Elapsed.TotalMicroseconds,
                Scale = _scale
            };

            MetricLogger.Log(metric);
            return result ?? Enumerable.Empty<T>();
        }

        public async Task<bool> MeasureUpdateAsync<T>(
           IRepository<T> repository,
           T entity,
           Func<T, Task<bool>> action) where T : class
        {
            var stopwatch = Stopwatch.StartNew();

            bool result = await action(entity);

            stopwatch.Stop();

            var metric = new MetricResult
            {
                Operation = action.Method.Name,
                Database = repository.DatabaseName,
                EntityType = typeof(T).Name,
                EntityCount = 1,
                ElapsedUs = (long)stopwatch.Elapsed.TotalMicroseconds,
                Scale = _scale
            };

            MetricLogger.Log(metric);
            return result;
        }

        public async Task<bool> MeasureDeleteAsync<T>(
           IRepository<T> repository,
           Guid id,
           Func<Guid, Task<bool>> action) where T : class
        {
            var stopwatch = Stopwatch.StartNew();

            bool result = await action(id);

            stopwatch.Stop();

            var metric = new MetricResult
            {
                Operation = action.Method.Name,
                Database = repository.DatabaseName,
                EntityType = typeof(T).Name,
                EntityCount = 1,
                ElapsedUs = (long)stopwatch.Elapsed.TotalMicroseconds,
                Scale = _scale
            };

            MetricLogger.Log(metric);
            return result;
        }

        public async Task<int> MeasureDeleteAllAsync<T>(
           IRepository<T> repository,
           Func<Task<int>> action) where T : class
        {
            var stopwatch = Stopwatch.StartNew();

            int result = await action();

            stopwatch.Stop();

            var metric = new MetricResult
            {
                Operation = action.Method.Name,
                Database = repository.DatabaseName,
                EntityType = typeof(T).Name,
                EntityCount = result,
                ElapsedUs = (long)stopwatch.Elapsed.TotalMicroseconds,
                Scale = _scale
            };

            MetricLogger.Log(metric);
            return result;
        }
    }
}
