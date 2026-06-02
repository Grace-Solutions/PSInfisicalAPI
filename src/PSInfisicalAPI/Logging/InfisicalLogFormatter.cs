using System;
using System.Globalization;

namespace PSInfisicalAPI.Logging
{
    public static class InfisicalLogFormatter
    {
        public static string Format(DateTimeOffset utcTimestamp, InfisicalLogLevel level, string component, string message)
        {
            string timestamp = utcTimestamp.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ", CultureInfo.InvariantCulture);
            string normalizedComponent = string.IsNullOrEmpty(component) ? "Unspecified" : component;
            string normalizedMessage = message ?? string.Empty;

            return string.Concat("[", timestamp, "] - [", level.ToString(), "] - [", normalizedComponent, "] - ", normalizedMessage);
        }

        public static string FormatNow(InfisicalLogLevel level, string component, string message)
        {
            return Format(DateTimeOffset.UtcNow, level, component, message);
        }
    }
}
