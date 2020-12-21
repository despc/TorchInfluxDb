namespace InfluxDb.Client
{
    public interface IInfluxDbAuthConfig
    {
        string HostUrl { get; }
        string Bucket { get; }
        string Organization { get; }
        string AuthenticationToken { get; }
    }
}