using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using InfluxDb.Client.Write;
using NUnit.Framework;

namespace InfluxDb.Test
{
    [TestFixture]
    public class Tests
    {
        InfluxDbWriteEndpoints _writeEndpoints;

        [SetUp]
        public void SetUp()
        {
            // InfluxDB instance must be running for this test suite
            if (!Process.GetProcessesByName("influxd").Any())
            {
                throw new Exception("InfluxDB instance not running");
            }

            var config = new ConfigImpl
            {
                HostUrl = "http://localhost:8086",
                Bucket = "gaalsien", // This bucket must be present in the DB instance
                Username = "",
                Password = ""
            };

            _writeEndpoints = new InfluxDbWriteEndpoints(config);
        }

        [TearDown]
        public void TearDown()
        {
            _writeEndpoints.Dispose();
        }

        [Test]
        public void WriteEndpoints_Write()
        {
            var lines = new List<string>
            {
                // just random data
                "foo bar=0i,baz=2 1605030124039",
            };

            // Will throw if anything went wrong
            _writeEndpoints.WriteAsync(lines).Wait();
        }
    }
}