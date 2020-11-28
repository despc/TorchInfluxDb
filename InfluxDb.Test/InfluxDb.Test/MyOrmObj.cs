using System;
using InfluxDb.Client.Orm;

namespace InfluxDb.Test
{
    public sealed class MyOrmObj
    {
        [FieldName("time")]
        public DateTime Time { get; set; }
        
        [FieldName("online_time")]
        public int OnlineTime { get; set; }

        public override string ToString()
        {
            return $"{nameof(Time)}: {Time}, {nameof(OnlineTime)}: {OnlineTime}";
        }
    }
}