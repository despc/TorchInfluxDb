using System;

namespace InfluxDb.Client.Orm
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class TagAttribute : Attribute
    {
        public TagAttribute(string tagName)
        {
            TagName = tagName;
        }

        public string TagName { get; }
    }
}