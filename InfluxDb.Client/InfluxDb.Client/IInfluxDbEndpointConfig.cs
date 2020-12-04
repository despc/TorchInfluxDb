namespace InfluxDb.Client
{
    public interface IInfluxDbEndpointConfig
    {
        string HostUrl { get; }
        string Bucket { get; }
        string Username { get; }
        string Password { get; }
    }
}