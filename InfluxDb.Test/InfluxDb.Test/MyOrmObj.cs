using System;
using InfluxDb.Client.Orm;

namespace InfluxDb.Test
{
    public sealed class MyOrmObj
    {

        [Field("time")]
        public DateTime Time { get; private set; }

        [Field("online_time")]
        public int OnlineTime { get; private set; }

        public override string ToString()
        {
            return $"{nameof(Time)}: {Time}, {nameof(OnlineTime)}: {OnlineTime}";
        }
    }
}