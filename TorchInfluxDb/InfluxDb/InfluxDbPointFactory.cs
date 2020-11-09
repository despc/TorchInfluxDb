using System;
using System.Threading.Tasks;
using InfluxDb.Impl;
using NLog;
using TorchUtils;

namespace InfluxDb
{
    /// <summary>
    /// Massive syntax sugar
    /// </summary>
    public static class InfluxDbPointFactory
    {
        static readonly ILogger Log = LogManager.GetCurrentClassLogger();
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

        public static void Write(this InfluxDbPoint point)
        {
            point.ThrowIfNull(nameof(point));
            WriteClient.ThrowIfNull("Integration not initialized");
            if (!Enabled) return;

            WriteClient.WriteAsync(point).Forget(Log);
        }

        public static async Task WriteAsync(this InfluxDbPoint point)
        {
            point.ThrowIfNull(nameof(point));
            WriteClient.ThrowIfNull("Integration not initialized");
            if (!Enabled) return;

            await WriteClient.WriteAsync(point);
        }
    }
}