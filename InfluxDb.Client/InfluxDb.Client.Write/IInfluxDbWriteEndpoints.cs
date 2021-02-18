using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InfluxDb.Client.Write
{
    public interface IInfluxDbWriteEndpoints : IDisposable
    {
        Task WriteAsync(IReadOnlyCollection<string> lines);
    }
}