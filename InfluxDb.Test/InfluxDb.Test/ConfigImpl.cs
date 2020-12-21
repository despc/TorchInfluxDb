using InfluxDb.Client;

namespace InfluxDb.Test
{
    public sealed class ConfigImpl : IInfluxDbAuthConfig
    {
        public string HostUrl { get; set; }
        public string Bucket { get; set; }
        public string Organization { get; set; }
        public string AuthenticationToken { get; set; }
    }
}