using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;

namespace InfluxDb.Client.Write
{
    public sealed class InfluxDbWriteClient : IInfluxDbWriteClient
    {
        public interface IConfig
        {
            bool SuppressResponseError { get; }
        }

        static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        readonly InfluxDbWriteEndpoints _endpoints;
        readonly IConfig _config;

        public InfluxDbWriteClient(InfluxDbWriteEndpoints endpoints, IConfig config)
        {
            _endpoints = endpoints;
            _config = config;
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
                if (_config.SuppressResponseError) return;

                Log.Error(e.Message);
            }
        }
    }
}