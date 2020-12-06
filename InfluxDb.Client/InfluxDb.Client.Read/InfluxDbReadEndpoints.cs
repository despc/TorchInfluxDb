using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        readonly IInfluxDbEndpointConfig _config;
        readonly HttpClient _httpClient;
        readonly CancellationTokenSource _cancellationTokenSource;

        public InfluxDbReadEndpoints(IInfluxDbEndpointConfig config)
        {
            _config = config;
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

            _config.HostUrl.ThrowIfNullOrEmpty(nameof(_config.HostUrl));
            _config.Bucket.ThrowIfNullOrEmpty(nameof(_config.Bucket));

            var urlEncodedQuery = WebUtility.UrlEncode(query);
            var url = $"{_config.HostUrl}/query?org={_config.Organization}&bucket={_config.Bucket}&q={urlEncodedQuery}&precision=ms";

            var req = new HttpRequestMessage(HttpMethod.Get, url);

            // authenticate
            if (!string.IsNullOrEmpty(_config.AuthenticationToken))
            {
                req.Headers.TryAddWithoutValidation("Authorization", $"Token {_config.AuthenticationToken}");
            }

            using (var res = await _httpClient.SendAsync(req, _cancellationTokenSource.Token).ConfigureAwait(false))
            {
                // Handle error
                if (!res.IsSuccessStatusCode)
                {
                    var msgBuilder = new StringBuilder();

                    msgBuilder.AppendLine($"Failed to write ({res.StatusCode});");
                    msgBuilder.AppendLine($"Host URL: {_config.HostUrl}");
                    msgBuilder.AppendLine($"Bucket: {_config.Bucket}");
                    msgBuilder.AppendLine($"Organization: {_config.Organization}");
                    msgBuilder.AppendLine($"Query: {query}");

                    if (_config.AuthenticationToken != null)
                    {
                        msgBuilder.AppendLine($"Authentication Token: {_config.AuthenticationToken.HideCredential(4)}");
                    }

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