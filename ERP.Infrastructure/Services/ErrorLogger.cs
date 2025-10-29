using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Net.Http;

namespace ERP.Infrastructure.Services
{
    public interface IErrorLogger
    {
        void Log(Exception ex, HttpContext? httpContext = null, string? Title = null);
    }

    public class ErrorLogger : IErrorLogger
    {
        private readonly string _logFilePath;

        public ErrorLogger()
        {
            //_logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "ErrorLogs.txt");
            _logFilePath = @"D:\Software Projects\ERP\ErrorLogs.txt";
            Directory.CreateDirectory(Path.GetDirectoryName(_logFilePath)!);
        }

        public void Log(Exception ex, HttpContext? httpContext = null, string? Title = null)
        {
            try
            {
                string log = $"WEBERP | [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {Title}\n" +
                             $"Error: {ex.Message}\n";
                             //$"StackTrace: {ex.StackTrace}\n";

                if (httpContext != null)
                {
                    log += $"URL: {httpContext.Request.Path}\n" +
                           $"User: {httpContext.User?.Identity?.Name ?? "Anonymous"}\n";
                }
                else
                {
                    log += $"URL: -\n" +
                           $"User: -\n";
                }

                log += "-------------------------\n";

                File.AppendAllText(_logFilePath, log);
            }
            catch
            {
                // Do not throw if logging fails
            }
        }

        public (string[] lines, int totalCount) ReadLogs(int page = 1, int pageSize = 50, string search = null, string severity = null, DateTime? from = null, DateTime? to = null)
        {
            if (!File.Exists(_logFilePath))
                return (Array.Empty<string>(), 0);

            var logs = File.ReadLines(_logFilePath);

            // Apply filters
            if (!string.IsNullOrEmpty(search))
                logs = logs.Where(line => line.Contains(search, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(severity))
                logs = logs.Where(line => line.Contains($"| {severity} |", StringComparison.OrdinalIgnoreCase));

            if (from.HasValue || to.HasValue)
                logs = logs.Where(line =>
                {
                    if (DateTime.TryParse(line.Substring(0, 19), out var dt))
                    {
                        return (!from.HasValue || dt >= from.Value) && (!to.HasValue || dt <= to.Value);
                    }
                    return false;
                });

            var total = logs.Count();
            var pageData = logs.Skip((page - 1) * pageSize).Take(pageSize).ToArray();

            return (pageData, total);
        }
    }
}
