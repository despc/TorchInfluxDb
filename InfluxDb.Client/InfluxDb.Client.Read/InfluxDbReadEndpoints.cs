using System;
using System.Collections.Generic;
using System.Data;
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

        public async Task<IEnumerable<InfluxDbSeries>> ReadAsync(string query)
        {
            query.ThrowIfNull(nameof(query));

            _config.HostUrl.ThrowIfNullOrEmpty(nameof(_config.HostUrl));
            _config.Bucket.ThrowIfNullOrEmpty(nameof(_config.Bucket));

            var urlEncodedQuery = WebUtility.UrlEncode(query);
            var url = $"{_config.HostUrl}/query?db={_config.Bucket}&q={urlEncodedQuery}&precision=ms";

            // authenticate
            if (!string.IsNullOrEmpty(_config.Username) && !string.IsNullOrEmpty(_config.Password))
            {
                url += $"&u={_config.Username}&p={_config.Password}";
            }

            var req = new HttpRequestMessage(HttpMethod.Get, url);

            using (var res = await _httpClient.SendAsync(req, _cancellationTokenSource.Token).ConfigureAwait(false))
            {
                if (!res.IsSuccessStatusCode)
                {
                    var msgBuilder = new StringBuilder();

                    msgBuilder.AppendLine($"Failed to write ({res.StatusCode});");
                    msgBuilder.AppendLine($"Host URL: {_config.HostUrl}");
                    msgBuilder.AppendLine($"Bucket: {_config.Bucket}");
                    msgBuilder.AppendLine($"Username: {_config.Username ?? "<empty>"}");
                    msgBuilder.AppendLine($"Password: {_config.Password ?? "<empty>"}");
                    msgBuilder.AppendLine($"Query: {query}");

                    throw new Exception(msgBuilder.ToString());
                }

                var contentText = await res.Content.ReadAsStringAsync();
                var contentToken = JToken.Parse(contentText);

                var seriesList = new List<InfluxDbSeries>();
                foreach (var resultToken in contentToken["results"])
                foreach (var seriesToken in resultToken["series"])
                {
                    var name = seriesToken["name"].Value<string>();
                    var tags = seriesToken["tags"].ToObject<Dictionary<string, string>>();
                    var columns = seriesToken["columns"];
                    var values = seriesToken["values"];

                    // make table
                    var table = new DataTable();

                    foreach (var column in columns)
                    {
                        table.Columns.Add(column.ToString());
                    }

                    foreach (var rowValues in values)
                    {
                        var row = rowValues.Select(v => v.ToObject<object>()).ToArray();
                        table.Rows.Add(row);
                    }

                    seriesList.Add(new InfluxDbSeries(name, tags, table));
                }

                return seriesList;
            }
        }
    }
}