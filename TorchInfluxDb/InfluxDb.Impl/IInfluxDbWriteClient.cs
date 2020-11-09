using System.Collections.Generic;
using System.Threading.Tasks;

namespace InfluxDb.Impl
{
    internal interface IInfluxDbWriteClient
    {
        Task WriteAsync(IEnumerable<InfluxDbPoint> points);
        Task WriteAsync(InfluxDbPoint point);
    }
}