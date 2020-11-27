using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using InfluxDb.Client.Read;
using InfluxDb.Client.Write;
using NUnit.Framework;

namespace InfluxDb.Test
{
    [TestFixture]
    public class Tests
    {
        InfluxDbWriteEndpoints _writeEndpoints;
        InfluxDbReadEndpoints _readEndpoints;

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
            _readEndpoints = new InfluxDbReadEndpoints(config);
        }

        [TearDown]
        public void TearDown()
        {
            _writeEndpoints.Dispose();
            _readEndpoints.Dispose();
        }

        [Test]
        public void WriteEndpoints_Write()
        {
            var lines = new List<string>
            {
                // just random data
                "players_churn,player_name=ryo0ka online_time=0",
                "players_churn,player_name=sama online_time=0",
            };

            // Will throw if anything went wrong
            _writeEndpoints.WriteAsync(lines).Wait();
        }

        [Test]
        public void ReadEndpoints_Read()
        {
            var query = "select * from players_churn group by player_name";

            // Will throw if anything went wrong
            var result = _readEndpoints.ReadAsync(query).Result;
            Console.WriteLine(result);
        }
    }
}