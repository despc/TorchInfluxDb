using InfluxDb.Client.Write;

namespace InfluxDb.Test
{
    public sealed class ConfigImpl : InfluxDbWriteEndpoints.IConfig
    {
        public string HostUrl { get; set; }
        public string Bucket { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}