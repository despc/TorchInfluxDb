using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utils.General;

namespace InfluxDb.Client.Write
{
    // https://docs.influxdata.com/influxdb/v1.8/tools/api
    public sealed class InfluxDbWriteEndpoints : IDisposable
    {
        readonly IInfluxDbEndpointConfig _config;
        readonly HttpClient _httpClient;
        readonly CancellationTokenSource _cancellationTokenSource;

        public InfluxDbWriteEndpoints(IInfluxDbEndpointConfig config)
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

        public async Task WriteAsync(IReadOnlyCollection<string> lines)
        {
            lines.ThrowIfNull(nameof(lines));

            foreach (var line in lines)
            {
                line.ThrowIfNullOrEmpty(nameof(line));
            }

            _config.HostUrl.ThrowIfNullOrEmpty(nameof(_config.HostUrl));
            _config.Bucket.ThrowIfNullOrEmpty(nameof(_config.Bucket));

            var url = $"{_config.HostUrl}/write?db={_config.Bucket}&precision=ms";

            // authenticate
            if (!string.IsNullOrEmpty(_config.Username) && !string.IsNullOrEmpty(_config.Password))
            {
                url += $"&u={_config.Username}&p={_config.Password}";
            }

            var req = new HttpRequestMessage(HttpMethod.Post, url);

            var content = string.Join("\n", lines);
            req.Content = new StringContent(content);

            using (var res = await _httpClient.SendAsync(req, _cancellationTokenSource.Token).ConfigureAwait(false))
            {
                if (res.IsSuccessStatusCode) return; // success

                var msgBuilder = new StringBuilder();

                msgBuilder.AppendLine($"Failed to write ({res.StatusCode});");
                msgBuilder.AppendLine($"Host URL: {_config.HostUrl}");
                msgBuilder.AppendLine($"Bucket: {_config.Bucket}");
                msgBuilder.AppendLine($"Username: {_config.Username ?? "<empty>"}");
                msgBuilder.AppendLine($"Password: {_config.Password ?? "<empty>"}");
                msgBuilder.AppendLine($"Content ({lines.Count} lines): ");

                foreach (var line in lines)
                {
                    msgBuilder.AppendLine($"Line: {line}");
                }

                throw new Exception(msgBuilder.ToString());
            }
        }
    }
}