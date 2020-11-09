namespace InfluxDb.Impl
{
    internal interface IInfluxDbConfig
    {
        string HostUrl { get; }
        string Organization { get; }
        string Bucket { get; }
        string AuthenticationToken { get; }
        bool SuppressResponseError { get; }
    }
}