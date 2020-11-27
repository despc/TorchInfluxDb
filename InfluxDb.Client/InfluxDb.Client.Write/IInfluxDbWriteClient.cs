using System.Collections.Generic;
using System.Threading.Tasks;

namespace InfluxDb.Client.Write
{
    public interface IInfluxDbWriteClient
    {
        Task WriteAsync(IEnumerable<InfluxDbPoint> points);
        Task WriteAsync(InfluxDbPoint point);
    }
}