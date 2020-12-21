using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Utils.General;

namespace InfluxDb.Client.Read
{
    public sealed class InfluxDbReadEndpoints : IDisposable
    {
        readonly InfluxDbAuth _auth;
        readonly HttpClient _httpClient;
        readonly CancellationTokenSource _cancellationTokenSource;

        public InfluxDbReadEndpoints(InfluxDbAuth auth)
        {
            _auth = auth;
            _httpClient = new HttpClient();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _httpClient.Dispose();
        }

        public async Task<IEnumerable<InfluxDbSeries>> QueryQlAsync(string query)
        {
            query.ThrowIfNull(nameof(query));

            _auth.ValidateOrThrow();

            var urlBuilder = _auth.MakeHttpUrlBuilder();
            urlBuilder.SetPath("query");
            urlBuilder.AddArgument("precision", "ms");
            urlBuilder.AddArgument("q", query);

            var req = new HttpRequestMessage(HttpMethod.Get, urlBuilder.ToUri());
            _auth.AuthenticateHttpRequest(req);

            using (var res = await _httpClient.SendAsync(req, _cancellationTokenSource.Token).ConfigureAwait(false))
            {
                // Handle error
                if (!res.IsSuccessStatusCode)
                {
                    var msgBuilder = new StringBuilder();

                    msgBuilder.AppendLine($"Failed to write ({res.StatusCode});");
                    msgBuilder.AppendLine($"{_auth}");
                    msgBuilder.AppendLine($"Query: {query}");

                    throw new Exception(msgBuilder.ToString());
                }

                var contentText = await res.Content.ReadAsStringAsync();
                return DeserializeResult(contentText);
            }
        }

        IEnumerable<InfluxDbSeries> DeserializeResult(string contentText)
        {
            var contentToken = JToken.Parse(contentText);

            var seriesList = new List<InfluxDbSeries>();
            foreach (var resultToken in contentToken["results"])
            foreach (var seriesToken in resultToken["series"])
            {
                var name = seriesToken["name"].Value<string>();
                var tags = seriesToken["tags"].ToObject<Dictionary<string, string>>();
                var columns = seriesToken["columns"].Select(c => c.ToString()).ToArray();
                var values = seriesToken["values"].Select(r => r.Select(v => v.ToObject<object>()).ToArray()).ToArray();
                var series = new InfluxDbSeries(name, tags, columns, values);
                seriesList.Add(series);
            }

            return seriesList;
        }
    }
}