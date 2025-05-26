using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbPerformanceComparison.Monitoring
{
    public class MetricLogger
    {
        private string FilePath = Path.Combine("output","metrics.csv");

        public MetricLogger(bool deleteFile = false)
        {
            if (deleteFile && File.Exists(FilePath))
            {
                File.Delete(FilePath);
            }

            if (!File.Exists(FilePath))
            {
                using StreamWriter writer = new(FilePath, append: true);
                writer.WriteLine("Operation,Database,ElapsedUs,EntityType,EntityCount,Date");
            }
        }

        public void Log(MetricResult result)
        {
            using var writer = new StreamWriter(FilePath, append: true);

            writer.WriteLine(result.ToString());
        }
    }

}
