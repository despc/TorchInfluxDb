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
            var fields = new List<(PropertyInfo, int)>();

            foreach (var propertyInfo in typeof(T).GetProperties())
            {
                if (!TryGetAttribute<FieldAttribute>(propertyInfo, out var fieldNameAttr)) continue;

                var fieldName = fieldNameAttr.FieldName;
                if (!TryGetOrdinalByName(table.Columns, fieldName, out var ordinal)) continue;

                fields.Add((propertyInfo, ordinal));
            }

            foreach (DataRow row in table.Rows)
            {
                var obj = new T();

                foreach (var (fieldInfo, ordinal) in fields)
                {
                    var value = row[ordinal].ToString();
                    SetPropertyValue(obj, fieldInfo, value);
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

        void SetPropertyValue(object propertyOwner, PropertyInfo propertyInfo, string stringValue)
        {
            var value = ParsePropertyValue(propertyInfo.PropertyType, stringValue);
            propertyInfo.SetValue(propertyOwner, value);
        }

        object ParsePropertyValue(Type targetType, string stringValue)
        {
            if (targetType == typeof(string)) return stringValue;
            if (targetType == typeof(byte)) return byte.Parse(stringValue);
            if (targetType == typeof(short)) return short.Parse(stringValue);
            if (targetType == typeof(ushort)) return ushort.Parse(stringValue);
            if (targetType == typeof(int)) return int.Parse(stringValue);
            if (targetType == typeof(uint)) return uint.Parse(stringValue);
            if (targetType == typeof(long)) return long.Parse(stringValue);
            if (targetType == typeof(ulong)) return ulong.Parse(stringValue);
            if (targetType == typeof(float)) return float.Parse(stringValue);
            if (targetType == typeof(double)) return double.Parse(stringValue);
            if (targetType == typeof(DateTime)) return DateTime.Parse(stringValue);

            throw new ArgumentException($"Unsupported type for parsing: {targetType}");
        }
    }
}