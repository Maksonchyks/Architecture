using System.Text;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Nimble.Modulith.Reporting.Endpoints;

public static class CsvFormatter
{
    public static async Task SendCsvAsync<T>(this IEndpoint ep, IEnumerable<T> records, CancellationToken ct)
    {
        ep.HttpContext.Response.ContentType = "text/csv";
        var props = typeof(T).GetProperties();
        
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", props.Select(p => EscapeCsv(p.Name))));
        
        foreach (var record in records)
        {
            var values = props.Select(p => 
            {
                var val = p.GetValue(record)?.ToString() ?? string.Empty;
                return EscapeCsv(val);
            });
            sb.AppendLine(string.Join(",", values));
        }
        
        await ep.HttpContext.Response.WriteAsync(sb.ToString(), ct);
    }

    private static string EscapeCsv(string field)
    {
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        return field;
    }
}
