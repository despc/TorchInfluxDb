using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Utils.General;

namespace InfluxDb.Client
{
    /// <summary>
    /// Holds onto points until the next time interval.
    /// Reduces the number of HTTP calls.
    /// </summary>
    public sealed class ThrottledInfluxDbWriteClient : IInfluxDbWriteClient
    {
        static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        readonly IInfluxDbWriteClient _self;
        readonly ThreadSafeThrottle<InfluxDbPoint> _throttle;

        public ThrottledInfluxDbWriteClient(IInfluxDbWriteClient self, TimeSpan throttleInterval)
        {
            _self = self;

            _throttle = new ThreadSafeThrottle<InfluxDbPoint>(
                throttleInterval,
                ps => OnThrottleFlush(ps));
        }

        public Task WriteAsync(IEnumerable<InfluxDbPoint> points)
        {
            points.ThrowIfNull(nameof(points));

            foreach (var point in points)
            {
                point.ThrowIfNull(nameof(point));
                _throttle.Add(point);
            }

            return Task.CompletedTask;
        }

        public Task WriteAsync(InfluxDbPoint point)
        {
            point.ThrowIfNull(nameof(point));

            _throttle.Add(point);

            return Task.CompletedTask;
        }

        void OnThrottleFlush(IEnumerable<InfluxDbPoint> points)
        {
            _self.WriteAsync(points).Forget(Log);
        }

        public void SetRunning(bool enable)
        {
            if (enable)
            {
                StartWriting();
            }
            else
            {
                StopWriting();
            }
        }

        public void StartWriting()
        {
            if (!_throttle.Start())
            {
                Log.Warn("Aborted; failed to start");
            }
        }

        public void StopWriting()
        {
            if (!_throttle.Stop())
            {
                Log.Warn("Aborted; failed to stop");
                return;
            }

            Flush();
        }

        public void Flush()
        {
            _throttle.Flush(); // write remaining points
            Thread.Sleep(1000); // wait for writes to finish
        }
    }
}