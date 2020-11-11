using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TorchUtils;

namespace InfluxDb.Client
{
    // https://docs.influxdata.com/influxdb/v2.0/write-data/developer-tools/api/
    public sealed class InfluxDbWriteEndpoints : IDisposable
    {
        public interface IConfig
        {
            string HostUrl { get; }
            string Organization { get; }
            string Bucket { get; }
            string AuthenticationToken { get; }
        }
        
        readonly IConfig _config;
        readonly HttpClient _httpClient;
        readonly CancellationTokenSource _cancellationTokenSource;

        public InfluxDbWriteEndpoints(IConfig config)
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
            _config.Organization.ThrowIfNullOrEmpty(nameof(_config.Organization));
            _config.Bucket.ThrowIfNullOrEmpty(nameof(_config.Bucket));

            var url = $"{_config.HostUrl}/api/v2/write?org={_config.Organization}&bucket={_config.Bucket}&precision=ms";
            var req = new HttpRequestMessage(HttpMethod.Post, url);

            var content = string.Join("\n", lines);
            req.Content = new StringContent(content);

            if (!string.IsNullOrEmpty(_config.AuthenticationToken))
            {
                req.Headers.TryAddWithoutValidation("Authorization", $"Token {_config.AuthenticationToken}");
            }

            using (var res = await _httpClient.SendAsync(req, _cancellationTokenSource.Token).ConfigureAwait(false))
            {
                if (res.IsSuccessStatusCode) return; // success

                var msgBuilder = new StringBuilder();

                msgBuilder.AppendLine($"Failed to write ({res.StatusCode});");
                msgBuilder.AppendLine($"Host URL: {_config.HostUrl}");
                msgBuilder.AppendLine($"Bucket: {_config.Bucket}");
                msgBuilder.AppendLine($"Organization: {_config.Organization}");

                if (_config.AuthenticationToken != null)
                {
                    msgBuilder.AppendLine($"Authentication Token: {_config.AuthenticationToken.HideCredential(4)}");
                }

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