using System.Collections.Generic;

namespace PSInfisicalAPI.Common
{
    public static class InfisicalSanitizer
    {
        private static readonly string[] SensitiveTokens = new[]
        {
            "secretValue",
            "secret_value",
            "clientSecret",
            "client_secret",
            "accessToken",
            "access_token",
            "Authorization",
            "Bearer"
        };

        public static string SanitizeBody(string body, bool containsSecretMaterial)
        {
            if (string.IsNullOrEmpty(body))
            {
                return body;
            }

            if (containsSecretMaterial)
            {
                return "[REDACTED]";
            }

            return Truncate(body, 1024);
        }

        public static string SanitizeHeaderValue(string headerName, string headerValue)
        {
            if (string.IsNullOrEmpty(headerName))
            {
                return headerValue;
            }

            string normalized = headerName.ToLowerInvariant();
            if (normalized == "authorization" || normalized.Contains("token") || normalized.Contains("secret"))
            {
                return "[REDACTED]";
            }

            return headerValue;
        }

        public static IDictionary<string, string> SanitizeHeaders(IDictionary<string, string> headers)
        {
            if (headers == null)
            {
                return null;
            }

            Dictionary<string, string> sanitized = new Dictionary<string, string>(headers.Count);

            foreach (KeyValuePair<string, string> entry in headers)
            {
                sanitized[entry.Key] = SanitizeHeaderValue(entry.Key, entry.Value);
            }

            return sanitized;
        }

        public static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            if (value.Length <= maxLength)
            {
                return value;
            }

            return string.Concat(value.Substring(0, maxLength), "... [truncated]");
        }
    }
}
