using System.Collections.Generic;
using System.Threading.Tasks;
using InfluxDb.Client.Orm;

namespace InfluxDb.Client.Read
{
    public sealed class InfluxDbReadClient : IInfluxDbReadClient
    {
        readonly InfluxDbReadEndpoints _endpoints;

        public InfluxDbReadClient(InfluxDbReadEndpoints endpoints)
        {
            _endpoints = endpoints;
        }

        public async Task<IEnumerable<T>> QueryQlAsync<T>(string query) where T : class, new()
        {
            var result = new List<T>();

            var response = await _endpoints.QueryQlAsync(query);
            foreach (var series in response)
            {
                var ormObjects = InfluxDbOrmFactory.Create<T>(series);
                foreach (var ormObject in ormObjects)
                {
                    result.Add(ormObject);
                }
            }

            return result;
        }
    }
}