using System;
using System.Collections.Generic;
using System.Linq;

namespace Utils.General
{
    public static class HttpUtils
    {
        public static string MakeUrl(string host, string path = null, IReadOnlyDictionary<string, string> strQueries = null)
        {
            host.ThrowIfNullOrEmpty(nameof(host));
            host = host.TrimEnd('/');
            path = path?.TrimStart('/');
            var query = MakeQuery(strQueries);
            var split = host[host.Length - 1] == '/' ? "" : "/";
            var url = $"{host}{split}{path}{query}";
            return url;
        }

        public static string MakeQuery(IReadOnlyDictionary<string, string> strQueriesOrNull)
        {
            if (strQueriesOrNull == null) return "";
            if (!strQueriesOrNull.Any()) return "";

            var queries = strQueriesOrNull.Select(q => $"{Uri.EscapeUriString(q.Key)}={Uri.EscapeUriString(q.Value)}");
            return "?" + string.Join("&", queries);
        }
    }
}