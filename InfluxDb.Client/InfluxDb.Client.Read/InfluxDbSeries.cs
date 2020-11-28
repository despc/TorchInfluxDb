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
            IReadOnlyList<string> columns,
            IReadOnlyList<IReadOnlyList<object>> values)
        {
            Name = name.OrThrow(nameof(name));
            Tags = tags.OrThrow(nameof(tags));
            Columns = columns.OrThrow(nameof(columns));
            Values = values.OrThrow(nameof(values));
        }

        public string Name { get; }
        public IReadOnlyDictionary<string, string> Tags { get; }
        public IReadOnlyList<string> Columns { get; }
        public IReadOnlyList<IReadOnlyList<object>> Values { get; }

        public DataTable CreateDataTable()
        {
            var table = new DataTable();

            foreach (var column in Columns)
            {
                table.Columns.Add(column);
            }

            foreach (var row in Values)
            {
                table.Rows.Add(row.AsArray());
            }

            return table;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine($"Name: {Name}");

            builder.Append("Tags: ");

            foreach (var (key, value) in Tags)
            {
                builder.Append($"{key} = {value}, ");
            }

            builder.Append("(tag end)");
            builder.AppendLine();

            var table = CreateDataTable();
            builder.AppendLine(table.ToStringTable());

            return builder.ToString();
        }
    }
}