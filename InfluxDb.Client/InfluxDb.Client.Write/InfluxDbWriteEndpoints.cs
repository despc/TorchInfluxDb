using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Utils.General;

namespace InfluxDb.Client.Write
{
    // https://docs.influxdata.com/influxdb/v2.0/write-data/developer-tools/api/
    public sealed class InfluxDbWriteEndpoints : IInfluxDbWriteEndpoints
    {
        static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        readonly InfluxDbAuth _auth;
        readonly HttpClient _httpClient;
        readonly CancellationTokenSource _cancellationTokenSource;

        public InfluxDbWriteEndpoints(InfluxDbAuth auth)
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

        public async Task WriteAsync(IReadOnlyCollection<string> lines)
        {
            lines.ThrowIfNull(nameof(lines));
            _auth.ValidateOrThrow();

            foreach (var line in lines)
            {
                line.ThrowIfNullOrEmpty(nameof(line));
            }

            var urlBuilder = _auth.MakeHttpUrlBuilder();
            urlBuilder.SetPath("api/v2/write");
            urlBuilder.AddArgument("precision", "ms");

            var url = urlBuilder.ToUri();

            Log.Debug($"Writing: URL: \"{url}\"");

            var req = new HttpRequestMessage(HttpMethod.Post, url);
            _auth.AuthenticateHttpRequest(req);

            var content = string.Join("\n", lines);
            req.Content = new StringContent(content);

            using (var res = await _httpClient.SendAsync(req, _cancellationTokenSource.Token).ConfigureAwait(false))
            {
                if (res.IsSuccessStatusCode)
                {
                    Log.Debug("Finished writing");
                    return;
                }

                switch (res.StatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                    {
                        var msgBuilder = new StringBuilder();
                        msgBuilder.AppendLine("Authentication failed");
                        msgBuilder.AppendLine($"{_auth}");
                        throw new Exception(msgBuilder.ToString());
                    }
                    case HttpStatusCode.Forbidden:
                    case HttpStatusCode.BadGateway:
                    case HttpStatusCode.NotFound:
                    {
                        var msgBuilder = new StringBuilder();
                        msgBuilder.AppendLine($"{res.StatusCode}");
                        msgBuilder.AppendLine($"{_auth}");
                        throw new Exception(msgBuilder.ToString());
                    }
                    default:
                    {
                        var msgBuilder = new StringBuilder();

                        msgBuilder.AppendLine($"{res.StatusCode}");
                        msgBuilder.AppendLine($"{_auth}");
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
    }
}