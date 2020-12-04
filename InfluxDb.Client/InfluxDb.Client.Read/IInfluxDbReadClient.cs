using System.Collections.Generic;
using System.Threading.Tasks;

namespace InfluxDb.Client.Read
{
    public interface IInfluxDbReadClient
    {
        Task<IEnumerable<T>> QueryQlAsync<T>(string query) where T : class, new();
    }
}