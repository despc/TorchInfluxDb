using System.Net.Http;
using System.Text;
using Utils.General;

namespace InfluxDb.Client
{
    public sealed class InfluxDbAuth
    {
        readonly IInfluxDbAuthConfig _config;

        public InfluxDbAuth(IInfluxDbAuthConfig config)
        {
            _config = config;
        }

        public void ValidateOrThrow()
        {
            _config.HostUrl.ThrowIfNullOrEmpty(nameof(_config.HostUrl));
            _config.Organization.ThrowIfNullOrEmpty(nameof(_config.Organization));
            _config.Bucket.ThrowIfNullOrEmpty(nameof(_config.Bucket));
        }

        public HttpUrlBuilder MakeHttpUrlBuilder()
        {
            var builder = new HttpUrlBuilder(_config.HostUrl);
            builder.AddArgument("org", _config.Organization);
            builder.AddArgument("bucket", _config.Bucket);
            return builder;
        }

        public void AuthenticateHttpRequest(HttpRequestMessage req)
        {
            // auth is not needed if disabled by the instance
            if (string.IsNullOrEmpty(_config.AuthenticationToken)) return;

            req.Headers.TryAddWithoutValidation("Authorization", $"Token {_config.AuthenticationToken}");
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Host URL: {_config.HostUrl}");
            sb.AppendLine($"Bucket: {_config.Bucket}");
            sb.AppendLine($"Organization: {_config.Organization}");

            if (_config.AuthenticationToken != null)
            {
                sb.AppendLine($"Authentication Token: {_config.AuthenticationToken.HideCredential(4)}");
            }

            return sb.ToString();
        }
    }
}