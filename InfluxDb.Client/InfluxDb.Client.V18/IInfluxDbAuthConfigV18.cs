namespace InfluxDb.Client.V18
{
    public interface IInfluxDbAuthConfigV18
    {
        string HostUrl { get; }
        string Bucket { get; }
        string Username { get; }
        string Password { get; }
    }
}