using System;
using System.Threading.Tasks;
using InfluxDb.Client.Write;
using NLog;
using Utils.General;

namespace TorchInfluxDb
{
    /// <summary>
    /// Massive syntax sugar
    /// </summary>
    public static class InfluxDbPointFactory
    {
        static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        internal static InfluxDbWriteEndpoints WriteEndpoints { private get; set; }
        internal static IInfluxDbWriteClient WriteClient { private get; set; }
        internal static bool Enabled { private get; set; }

        public static InfluxDbPoint Measurement(string measurement)
        {
            measurement.ThrowIfNullOrEmpty(nameof(measurement));
            WriteClient.ThrowIfNull("Integration not initialized");

            var point = new InfluxDbPoint(measurement);

            point.Time(DateTime.UtcNow); // default time

            return point;
        }

        public static async Task WriteLineAsync(string line)
        {
            line.ThrowIfNullOrEmpty(nameof(line));
            WriteClient.ThrowIfNull("Integration not initialized");
            if (!Enabled)
            {
                throw new Exception("Not enabled");
            }

            await WriteEndpoints.WriteAsync(new[] {line});
        }

        public static async Task WriteAsync(this InfluxDbPoint point)
        {
            point.ThrowIfNull(nameof(point));
            WriteClient.ThrowIfNull("Integration not initialized");
            if (!Enabled) return;

            await WriteClient.WriteAsync(point);
        }

        public static void Write(this InfluxDbPoint point)
        {
            point.WriteAsync().Forget(Log);
        }
    }
}