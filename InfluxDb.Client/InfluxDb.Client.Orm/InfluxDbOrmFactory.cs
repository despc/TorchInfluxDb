using System;
using System.Collections.Generic;
using InfluxDb.Client.Read;

namespace InfluxDb.Client.Orm
{
    public static class InfluxDbOrmFactory
    {
        static readonly Dictionary<Type, IInfluxDbOrmCreator<object>> _creators;

        static InfluxDbOrmFactory()
        {
            _creators = new Dictionary<Type, IInfluxDbOrmCreator<object>>();
        }

        public static IEnumerable<T> Create<T>(InfluxDbSeries series) where T : class, new()
        {
            if (!_creators.TryGetValue(typeof(T), out var creator))
            {
                creator = new InfluxDbOrmCreator<T>();
                _creators[typeof(T)] = creator;
            }

            var result = creator.Create(series);
            return (IEnumerable<T>) result;
        }
    }
}