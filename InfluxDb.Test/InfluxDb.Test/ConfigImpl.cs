using InfluxDb.Client;

namespace InfluxDb.Test
{
    public sealed class ConfigImpl : IInfluxDbAuthConfig
    {
        public string HostUrl { get; set; }
        public string Bucket { get; set; }
        public string Organization { get; set; }
        public string AuthenticationToken { get; set; }

        public bool UseV18 { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}