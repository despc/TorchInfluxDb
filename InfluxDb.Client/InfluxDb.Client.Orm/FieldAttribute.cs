using System;

namespace InfluxDb.Client.Orm
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class FieldAttribute : Attribute
    {
        public FieldAttribute(string fieldName)
        {
            FieldName = fieldName;
        }

        public string FieldName { get; }
    }
}