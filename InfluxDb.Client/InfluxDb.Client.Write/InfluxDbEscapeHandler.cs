using System.Text.RegularExpressions;

namespace InfluxDb.Client.Write
{
    // https://docs.influxdata.com/influxdb/v2.0/reference/syntax/line-protocol/#special-characters
    public static class InfluxDbEscapeHandler
    {
        const string Escape = @"\$1";
        const string MeasurementSpecialChars = @"([, ])";
        const string TagKeySpecialChars = @"([,= ])";
        const string TagValueSpecialChars = @"([,= ])";
        const string FieldKeySpecialChars = @"([,= ])";
        const string FieldValueSpecialChars = @"([""\\])";

        static string Handle(string input, string specialChars)
        {
            input = input.Replace('\\', '_'); // idk just lazy atm
            return Regex.Replace(input, specialChars, Escape);
        }

        public static string HandleMeasurement(string input)
        {
            return Handle(input, MeasurementSpecialChars);
        }

        public static string HandleTagKey(string input)
        {
            return Handle(input, TagKeySpecialChars);
        }

        public static string HandleTagValue(string input)
        {
            return Handle(input, TagValueSpecialChars);
        }

        public static string HandleFieldKey(string input)
        {
            return Handle(input, FieldKeySpecialChars);
        }

        public static string HandleFieldValue(string input)
        {
            return Handle(input, FieldValueSpecialChars);
        }
    }
}