namespace InfluxDb.Client
{
    public interface IInfluxDbEndpointConfig
    {
        string HostUrl { get; }
        string Bucket { get; }
        string Organization { get; }
        string AuthenticationToken { get; }
    }
}