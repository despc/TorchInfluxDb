using InfluxDb.Client;

namespace InfluxDb.Test
{
    public sealed class ConfigImpl : IInfluxDbEndpointConfig
    {
        public string HostUrl { get; set; }
        public string Bucket { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}