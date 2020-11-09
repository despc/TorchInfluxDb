using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InfluxDb.Impl
{
    internal sealed class InfluxDbWriteClient : IInfluxDbWriteClient
    {
        readonly InfluxDbWriteEndpoints _endpoints;

        public InfluxDbWriteClient(InfluxDbWriteEndpoints endpoints)
        {
            _endpoints = endpoints;
        }

        public async Task WriteAsync(IEnumerable<InfluxDbPoint> points)
        {
            var lines = points.Select(p => p.BuildLine());
            await _endpoints.WriteAsync(lines);
        }

        public async Task WriteAsync(InfluxDbPoint point)
        {
            var line = point.BuildLine();
            await _endpoints.WriteAsync(new[] {line});
        }
    }
}