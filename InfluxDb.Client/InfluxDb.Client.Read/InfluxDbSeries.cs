using System.Collections.Generic;
using System.Data;
using System.Text;
using Utils.General;

namespace InfluxDb.Client.Read
{
    public sealed class InfluxDbSeries
    {
        public InfluxDbSeries(
            string name,
            IReadOnlyDictionary<string, string> tags,
            DataTable table)
        {
            name.ThrowIfNull(nameof(name));
            tags.ThrowIfNull(nameof(tags));
            table.ThrowIfNull(nameof(table));

            Name = name;
            Tags = tags;
            Table = table;
        }

        public string Name { get; }
        public IReadOnlyDictionary<string, string> Tags { get; }
        public DataTable Table { get; }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine($"Name: {Name}");

            foreach (var (key, value) in Tags)
            {
                builder.Append($"{key} = {value}, ");
            }

            builder.Append("(tag end)");
            builder.AppendLine();

            builder.AppendLine(Table.ToStringTable());

            return builder.ToString();
        }
    }
}