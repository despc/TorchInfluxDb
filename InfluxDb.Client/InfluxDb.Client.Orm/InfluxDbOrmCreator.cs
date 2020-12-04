using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using InfluxDb.Client.Read;
using Utils.General;

namespace InfluxDb.Client.Orm
{
    public sealed class InfluxDbOrmCreator<T> : IInfluxDbOrmCreator<T> where T : new()
    {
        readonly List<(PropertyInfo, FieldAttribute)> _fieldProperties;
        readonly List<(PropertyInfo, TagAttribute)> _tagProperties;

        public InfluxDbOrmCreator()
        {
            _fieldProperties = new List<(PropertyInfo, FieldAttribute)>();
            _tagProperties = new List<(PropertyInfo, TagAttribute)>();

            foreach (var propertyInfo in typeof(T).GetProperties())
            {
                if (propertyInfo.TryGetAttribute(out FieldAttribute fieldAttr))
                {
                    _fieldProperties.Add((propertyInfo, fieldAttr));
                }

                if (propertyInfo.TryGetAttribute(out TagAttribute tagAttr))
                {
                    _tagProperties.Add((propertyInfo, tagAttr));
                }
            }
        }

        public IEnumerable<T> Create(InfluxDbSeries series)
        {
            var table = series.CreateDataTable();

            var tags = new List<(PropertyInfo, string)>();
            var fields = new List<(PropertyInfo, int)>();

            foreach (var (propertyInfo, tagAttribute) in _tagProperties)
            {
                var tagName = tagAttribute.TagName;
                if (series.Tags.TryGetValue(tagName, out var tagValue))
                {
                    tags.Add((propertyInfo, tagValue));
                }
            }

            foreach (var (propertyInfo, fieldAttribute) in _fieldProperties)
            {
                var fieldName = fieldAttribute.FieldName;
                if (table.Columns.TryGetOrdinalByName(fieldName, out var ordinal))
                {
                    fields.Add((propertyInfo, ordinal));
                }
            }

            // Inject values
            var ormObjects = new List<T>();
            foreach (DataRow row in table.Rows)
            {
                var obj = new T();

                foreach (var (propertyInfo, tagValue) in tags)
                {
                    propertyInfo.SetValue(obj, tagValue);
                }

                foreach (var (propertyInfo, ordinal) in fields)
                {
                    var fieldValueStr = row[ordinal].ToString();
                    var fieldValue = ParsePropertyValue(propertyInfo.PropertyType, fieldValueStr);
                    propertyInfo.SetValue(obj, fieldValue);
                }

                ormObjects.Add(obj);
            }

            return ormObjects;
        }

        static object ParsePropertyValue(Type targetType, string stringValue)
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