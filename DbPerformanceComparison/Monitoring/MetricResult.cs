using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbPerformanceComparison.Monitoring
{
    public class MetricResult
    {
        public string Operation { get; set; } = string.Empty;
        public string Database { get; set; } = string.Empty;
        public long ElapsedUs { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public int EntityCount { get; set; }
        
        public override string ToString()
        {
            return $"{Operation},{Database},{ElapsedUs},{EntityType},{EntityCount}";
        }
    }
}
