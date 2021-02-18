using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;

namespace InfluxDb.Client.Write
{
    public sealed class InfluxDbWriteClient
    {
        static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        readonly IInfluxDbWriteEndpoints _endpoints;

        public InfluxDbWriteClient(IInfluxDbWriteEndpoints endpoints)
        {
            _endpoints = endpoints;
        }

        public Task WriteAsync(IEnumerable<InfluxDbPoint> points)
        {
            var lines = points.Select(p => p.BuildLine()).ToArray();
            return DoWriteAsync(lines);
        }

        public Task WriteAsync(InfluxDbPoint point)
        {
            var line = point.BuildLine();
            return DoWriteAsync(new[] {line});
        }

        async Task DoWriteAsync(IReadOnlyCollection<string> lines)
        {
            try
            {
                await _endpoints.WriteAsync(lines);
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }
        }
    }
}