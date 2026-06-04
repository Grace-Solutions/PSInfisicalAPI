using System;
using Newtonsoft.Json.Linq;

namespace PSInfisicalAPI.Errors
{
    internal static class InfisicalApiErrorEnvelope
    {
        public static void Enrich(InfisicalApiException exception, string body)
        {
            if (exception == null || string.IsNullOrEmpty(body))
            {
                return;
            }

            string trimmed = body.TrimStart();
            if (trimmed.Length == 0 || (trimmed[0] != '{' && trimmed[0] != '['))
            {
                return;
            }

            JObject obj;
            try
            {
                JToken token = JToken.Parse(body);
                if (token.Type != JTokenType.Object) { return; }
                obj = (JObject)token;
            }
            catch (Exception)
            {
                return;
            }

            string message = ReadString(obj, "message");
            string error = ReadString(obj, "error");
            string reqId = ReadString(obj, "reqId");

            if (!string.IsNullOrEmpty(message)) { exception.ApiErrorMessage = message; }
            if (!string.IsNullOrEmpty(error) && string.IsNullOrEmpty(exception.ApiErrorCode)) { exception.ApiErrorCode = error; }
            if (!string.IsNullOrEmpty(reqId)) { exception.ApiRequestId = reqId; }
        }

        public static string BuildExceptionMessage(int statusCode, string reasonPhrase, string body)
        {
            string baseMessage = string.Concat(
                "Infisical API returned ",
                statusCode.ToString(System.Globalization.CultureInfo.InvariantCulture),
                " (", reasonPhrase ?? string.Empty, ").");

            string apiMessage = null;
            string apiError = null;
            string reqId = null;

            if (!string.IsNullOrEmpty(body))
            {
                string trimmed = body.TrimStart();
                if (trimmed.Length > 0 && trimmed[0] == '{')
                {
                    try
                    {
                        JToken token = JToken.Parse(body);
                        if (token.Type == JTokenType.Object)
                        {
                            JObject obj = (JObject)token;
                            apiMessage = ReadString(obj, "message");
                            apiError = ReadString(obj, "error");
                            reqId = ReadString(obj, "reqId");
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            if (string.IsNullOrEmpty(apiMessage) && string.IsNullOrEmpty(apiError) && string.IsNullOrEmpty(reqId))
            {
                return baseMessage;
            }

            System.Text.StringBuilder builder = new System.Text.StringBuilder(baseMessage);
            if (!string.IsNullOrEmpty(apiMessage))
            {
                builder.Append(' ').Append(apiMessage);
            }

            if (!string.IsNullOrEmpty(apiError) || !string.IsNullOrEmpty(reqId))
            {
                builder.Append(" [");
                bool needsSeparator = false;
                if (!string.IsNullOrEmpty(apiError))
                {
                    builder.Append("error=").Append(apiError);
                    needsSeparator = true;
                }

                if (!string.IsNullOrEmpty(reqId))
                {
                    if (needsSeparator) { builder.Append("; "); }
                    builder.Append("reqId=").Append(reqId);
                }

                builder.Append(']');
            }

            return builder.ToString();
        }

        private static string ReadString(JObject obj, string name)
        {
            JToken token;
            if (obj.TryGetValue(name, StringComparison.OrdinalIgnoreCase, out token) && token != null && token.Type == JTokenType.String)
            {
                return (string)token;
            }

            return null;
        }
    }
}
