using System;
using System.Collections.Generic;
using System.Text;
using PSInfisicalAPI.Endpoints;
using PSInfisicalAPI.Errors;

namespace PSInfisicalAPI.Http
{
    public static class InfisicalUriBuilder
    {
        public static Uri Build(Uri baseUri, InfisicalEndpointDefinition definition, IDictionary<string, string> pathParameters, IEnumerable<KeyValuePair<string, string>> queryParameters)
        {
            if (baseUri == null)
            {
                throw new InfisicalConfigurationException("Base URI must be provided.");
            }

            if (definition == null)
            {
                throw new InfisicalConfigurationException("Endpoint definition must be provided.");
            }

            string template = definition.Template ?? string.Empty;
            string resolvedPath = ResolvePathTemplate(template, pathParameters);

            UriBuilder uriBuilder = new UriBuilder(baseUri);
            uriBuilder.Path = CombinePaths(uriBuilder.Path, resolvedPath);
            uriBuilder.Query = BuildQueryString(queryParameters);

            return uriBuilder.Uri;
        }

        public static string BuildQueryString(IEnumerable<KeyValuePair<string, string>> queryParameters)
        {
            if (queryParameters == null)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            bool first = true;

            foreach (KeyValuePair<string, string> entry in queryParameters)
            {
                if (string.IsNullOrEmpty(entry.Key))
                {
                    continue;
                }

                if (entry.Value == null)
                {
                    continue;
                }

                if (!first)
                {
                    builder.Append('&');
                }

                builder.Append(Uri.EscapeDataString(entry.Key));
                builder.Append('=');
                builder.Append(Uri.EscapeDataString(entry.Value));
                first = false;
            }

            return builder.ToString();
        }

        public static string ResolvePathTemplate(string template, IDictionary<string, string> pathParameters)
        {
            if (string.IsNullOrEmpty(template))
            {
                return string.Empty;
            }

            if (pathParameters == null || pathParameters.Count == 0)
            {
                if (template.IndexOf('{') >= 0)
                {
                    throw new InfisicalConfigurationException(string.Concat("Path template '", template, "' requires parameters but none were supplied."));
                }

                return template;
            }

            string resolved = template;
            foreach (KeyValuePair<string, string> entry in pathParameters)
            {
                string token = string.Concat("{", entry.Key, "}");
                if (resolved.IndexOf(token, StringComparison.Ordinal) < 0)
                {
                    continue;
                }

                string escaped = Uri.EscapeDataString(entry.Value ?? string.Empty);
                resolved = resolved.Replace(token, escaped);
            }

            if (resolved.IndexOf('{') >= 0)
            {
                throw new InfisicalConfigurationException(string.Concat("Path template '", template, "' has unresolved tokens."));
            }

            return resolved;
        }

        private static string CombinePaths(string left, string right)
        {
            string l = string.IsNullOrEmpty(left) ? "/" : left;
            string r = right ?? string.Empty;

            if (l.EndsWith("/", StringComparison.Ordinal) && r.StartsWith("/", StringComparison.Ordinal))
            {
                return string.Concat(l, r.Substring(1));
            }

            if (!l.EndsWith("/", StringComparison.Ordinal) && !r.StartsWith("/", StringComparison.Ordinal))
            {
                return string.Concat(l, "/", r);
            }

            return string.Concat(l, r);
        }
    }
}
