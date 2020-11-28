using System;

namespace InfluxDb.Client.Orm
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class FieldNameAttribute : Attribute
    {
        public FieldNameAttribute(string fieldName)
        {
            FieldName = fieldName;
        }

        public string FieldName { get; }
    }
}