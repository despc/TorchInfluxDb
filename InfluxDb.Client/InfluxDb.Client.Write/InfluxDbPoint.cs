using System;
using System.Collections.Generic;
using NLog;
using Utils.General;

namespace InfluxDb.Client.Write
{
    /// <summary>
    /// Builder of an InfluxDB data point.
    /// </summary>
    /// <remarks>For the protocol, see: https://docs.influxdata.com/influxdb/v1.8/write_protocols/line_protocol_reference/</remarks>
    public sealed class InfluxDbPoint
    {
        static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        readonly string _measurement;
        readonly Dictionary<string, string> _tags;
        readonly Dictionary<string, string> _fields;

        string _time;

        public InfluxDbPoint(string measurement)
        {
            measurement.ThrowIfNullOrEmpty(nameof(measurement));
            AssertNamingRestriction(measurement);
            measurement = InfluxDbEscapeHandler.HandleMeasurement(measurement);

            _measurement = measurement;
            _tags = new Dictionary<string, string>();
            _fields = new Dictionary<string, string>();
        }

        public InfluxDbPoint Time(DateTime time)
        {
            _time = TimeToString(time);
            return this;
        }

        public InfluxDbPoint Tag(string key, string value)
        {
            key.ThrowIfNullOrEmpty(nameof(key));
            value.ThrowIfNull(nameof(value));
            AssertNamingRestriction(key);
            key = InfluxDbEscapeHandler.HandleTagKey(key);
            value = InfluxDbEscapeHandler.HandleTagValue(value);

            if (_tags.TryGetValue(key, out var existingValue))
            {
                throw new ArgumentException($"Tag already added: {key}, existing value: {existingValue}, measurement: {_measurement}");
            }

            _tags.Add(key, value);
            return this;
        }

        void AddFieldRaw(string key, string value)
        {
            if (_fields.TryGetValue(key, out var existingValue))
            {
                throw new ArgumentException($"Field already added: {key}, existing value: {existingValue}, measurement: {_measurement}");
            }

            _fields.Add(key, value);
        }

        public InfluxDbPoint Field(string key, string value)
        {
            key.ThrowIfNullOrEmpty(nameof(key));
            value.ThrowIfNull(nameof(value));
            AssertNamingRestriction(key);
            key = InfluxDbEscapeHandler.HandleFieldKey(key);
            value = InfluxDbEscapeHandler.HandleFieldValue(value);

            var valueStr = $"\"{value}\"";
            AddFieldRaw(key, valueStr);
            return this;
        }

        public InfluxDbPoint Field(string key, float value)
        {
            key.ThrowIfNullOrEmpty(nameof(key));
            AssertNamingRestriction(key);
            key = InfluxDbEscapeHandler.HandleFieldKey(key);

            if (float.IsInfinity(value) || float.IsNaN(value))
            {
                value = 0f;
                Log.Warn($"Infinity or NaN float value set to 0. '{_measurement}.{key}'");
            }

            var valueStr = $"{value}";
            AddFieldRaw(key, valueStr);
            return this;
        }

        public InfluxDbPoint Field(string key, double value)
        {
            key.ThrowIfNullOrEmpty(nameof(key));
            AssertNamingRestriction(key);
            key = InfluxDbEscapeHandler.HandleFieldKey(key);

            if (double.IsInfinity(value) || double.IsNaN(value))
            {
                value = 0;
                Log.Warn($"Infinity or NaN double value set to 0. '{_measurement}.{key}'");
            }

            var valueStr = $"{value}";
            AddFieldRaw(key, valueStr);
            return this;
        }

        public InfluxDbPoint Field(string key, int value)
        {
            key.ThrowIfNullOrEmpty(nameof(key));
            AssertNamingRestriction(key);
            key = InfluxDbEscapeHandler.HandleFieldKey(key);

            var valueStr = $"{value}i";
            AddFieldRaw(key, valueStr);
            return this;
        }

        public InfluxDbPoint Field(string key, uint value)
        {
            key.ThrowIfNullOrEmpty(nameof(key));
            AssertNamingRestriction(key);
            key = InfluxDbEscapeHandler.HandleFieldKey(key);

            var valueStr = $"{value}u";
            AddFieldRaw(key, valueStr);
            return this;
        }

        public InfluxDbPoint Field(string key, bool value)
        {
            key.ThrowIfNullOrEmpty(nameof(key));
            AssertNamingRestriction(key);
            key = InfluxDbEscapeHandler.HandleFieldKey(key);

            var valueStr = $"{value}";
            AddFieldRaw(key, valueStr);
            return this;
        }

        public InfluxDbPoint Field(string key, DateTime value)
        {
            key.ThrowIfNullOrEmpty(nameof(key));
            AssertNamingRestriction(key);
            key = InfluxDbEscapeHandler.HandleFieldKey(key);

            var valueStr = TimeToString(value);
            AddFieldRaw(key, valueStr);
            return this;
        }

        internal string BuildLine()
        {
            if (_fields.Count == 0)
            {
                throw new Exception($"No field. Measurement: {_measurement}, tags: {_tags.ToStringSeq()}");
            }

            var firstSection = new List<string>();
            var secondSection = new List<string>();

            firstSection.Add(_measurement);

            foreach (var tag in _tags)
            {
                firstSection.Add($"{tag.Key}={tag.Value}");
            }

            foreach (var field in _fields)
            {
                secondSection.Add($"{field.Key}={field.Value}");
            }

            return $"{string.Join(",", firstSection)} {string.Join(",", secondSection)} {_time}";
        }

        // https://docs.influxdata.com/influxdb/v2.0/reference/syntax/line-protocol/#naming-restrictions
        static void AssertNamingRestriction(string value)
        {
            if (value.StartsWith("_"))
            {
                throw new ArgumentException($"Cannot start with an underscore character: '{value}'");
            }
        }

        //https://docs.influxdata.com/influxdb/v2.0/reference/glossary/#unix-timestamp
        static string TimeToString(DateTime time)
        {
            return $"{(long) ((time - new DateTime(1970, 1, 1)).TotalSeconds * 1000)}";
        }
    }
}