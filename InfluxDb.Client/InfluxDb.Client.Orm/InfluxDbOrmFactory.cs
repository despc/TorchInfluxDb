using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using InfluxDb.Client.Read;

namespace InfluxDb.Client.Orm
{
    public sealed class InfluxDbOrmFactory
    {
        public static readonly InfluxDbOrmFactory Instance = new InfluxDbOrmFactory();

        InfluxDbOrmFactory()
        {
        }

        public IEnumerable<T> Create<T>(InfluxDbSeries series) where T : new()
        {
            var table = series.CreateDataTable();
            var objs = new List<T>();
            var fields = new List<(FieldInfo, int)>();

            foreach (var fieldInfo in typeof(T).GetFields())
            {
                if (!TryGetAttribute<FieldNameAttribute>(fieldInfo, out var fieldNameAttr)) continue;

                var fieldName = fieldNameAttr.FieldName;
                if (!TryGetOrdinalByName(table.Columns, fieldName, out var ordinal)) continue;

                fields.Add((fieldInfo, ordinal));
            }

            foreach (DataRow row in table.Rows)
            {
                var obj = new T();

                foreach (var (fieldInfo, ordinal) in fields)
                {
                    var value = row[ordinal];
                    fieldInfo.SetValue(obj, value);
                }

                objs.Add(obj);
            }

            return objs;
        }

        bool TryGetAttribute<T>(MemberInfo info, out T attribute) where T : Attribute
        {
            foreach (var customAttribute in info.GetCustomAttributes())
            {
                if (customAttribute.GetType() == typeof(T))
                {
                    attribute = (T) customAttribute;
                    return true;
                }
            }

            attribute = default;
            return false;
        }

        bool TryGetOrdinalByName(DataColumnCollection self, string name, out int ordinal)
        {
            if (self.Contains(name))
            {
                var column = self[name];
                ordinal = column.Ordinal;
                return true;
            }

            ordinal = default;
            return false;
        }
    }
}