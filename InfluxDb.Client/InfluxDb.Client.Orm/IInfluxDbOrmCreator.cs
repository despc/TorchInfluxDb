using System.Collections.Generic;
using InfluxDb.Client.Read;

namespace InfluxDb.Client.Orm
{
    public interface IInfluxDbOrmCreator<out T>
    {
        IEnumerable<T> Create(InfluxDbSeries series);
    }
}