using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NLog;
using Utils.General;

namespace InfluxDb.Client.Write
{
    /// <summary>
    /// Holds onto points until the next time interval.
    /// Reduces the number of HTTP calls.
    /// </summary>
    public sealed class ThrottledInfluxDbWriteClient
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

        public void Write(IEnumerable<InfluxDbPoint> points)
        {
            points.ThrowIfNull(nameof(points));

            foreach (var point in points)
            {
                point.ThrowIfNull(nameof(point));
                _throttle.Add(point);
            }
        }

        public void Write(InfluxDbPoint point)
        {
            point.ThrowIfNull(nameof(point));

            _throttle.Add(point);
        }

        void OnThrottleFlush(IEnumerable<InfluxDbPoint> points)
        {
            // don't send if empty
            if (!points.Any()) return;

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
                Log.Warn("Aborted starting; already started");
            }
        }

        public void StopWriting()
        {
            if (!_throttle.Stop())
            {
                Log.Warn("Aborted stopping; not running");
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