using System;
using System.Collections.Generic;
using System.Globalization;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Endpoints;
using PSInfisicalAPI.Errors;
using PSInfisicalAPI.Security;

namespace PSInfisicalAPI.Http
{
    internal sealed class InfisicalApiInvoker
    {
        private readonly IInfisicalHttpClient _httpClient;

        public InfisicalApiInvoker(IInfisicalHttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public InfisicalHttpResponse Invoke(
            InfisicalConnection connection,
            string endpointName,
            string operationName,
            IDictionary<string, string> pathParameters,
            IEnumerable<KeyValuePair<string, string>> queryParameters,
            string body)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            if (string.IsNullOrEmpty(endpointName)) { throw new ArgumentNullException(nameof(endpointName)); }

            InfisicalEndpointDefinition definition = InfisicalEndpointRegistry.Get(endpointName);
            Uri uri = InfisicalUriBuilder.Build(connection.BaseUri, definition, pathParameters, queryParameters);

            InfisicalHttpResponse response = ExecuteAuthorized(connection, definition, operationName, uri, body);

            if (response.StatusCode >= 200 && response.StatusCode < 300)
            {
                return response;
            }

            InfisicalApiException exception = BuildApiException(response, definition);
            response.Clear();
            throw exception;
        }

        public InfisicalHttpResponse InvokeWithCandidateFallback(
            InfisicalConnection connection,
            string endpointName,
            string operationName,
            IDictionary<string, string> pathParameters,
            IEnumerable<KeyValuePair<string, string>> queryParameters,
            string body)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            if (string.IsNullOrEmpty(endpointName)) { throw new ArgumentNullException(nameof(endpointName)); }

            IReadOnlyList<InfisicalEndpointDefinition> candidates = InfisicalEndpointRegistry.GetCandidates(endpointName);
            InfisicalApiException lastException = null;

            for (int index = 0; index < candidates.Count; index++)
            {
                InfisicalEndpointDefinition definition = candidates[index];
                Uri uri = InfisicalUriBuilder.Build(connection.BaseUri, definition, pathParameters, queryParameters);
                InfisicalHttpResponse response = ExecuteAuthorized(connection, definition, operationName, uri, body);

                if (response.StatusCode >= 200 && response.StatusCode < 300)
                {
                    return response;
                }

                InfisicalApiException exception = BuildApiException(response, definition);
                response.Clear();

                bool hasMoreCandidates = (index + 1) < candidates.Count;
                if (hasMoreCandidates && IsRouteAliasMismatch(exception))
                {
                    lastException = exception;
                    continue;
                }

                throw exception;
            }

            throw lastException ?? new InfisicalApiException(string.Concat(
                "All route candidates exhausted for '", endpointName, "'."));
        }

        private static bool IsRouteAliasMismatch(InfisicalApiException exception)
        {
            return exception.StatusCode == 404 || exception.StatusCode == 405;
        }

        private InfisicalHttpResponse ExecuteAuthorized(
            InfisicalConnection connection,
            InfisicalEndpointDefinition definition,
            string operationName,
            Uri uri,
            string body)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            headers["Accept"] = "application/json";

            if (!string.IsNullOrEmpty(body))
            {
                headers["Content-Type"] = "application/json";
            }

            if (definition.RequiresAuthorization)
            {
                if (connection.AccessToken == null)
                {
                    throw new InfisicalAuthenticationException("Connection does not contain an access token.");
                }

                SecureStringUtility.UsePlainText(connection.AccessToken, plainToken =>
                {
                    headers["Authorization"] = string.Concat("Bearer ", plainToken ?? string.Empty);
                });
            }

            InfisicalHttpRequest request = new InfisicalHttpRequest
            {
                OperationName = operationName,
                EndpointName = definition.Name,
                Method = definition.Method,
                Uri = uri,
                Headers = headers,
                Body = body,
                ContainsSecretMaterialInRequest = definition.ContainsSecretMaterialInRequest,
                ContainsSecretMaterialInResponse = definition.ContainsSecretMaterialInResponse
            };

            return _httpClient.Send(request);
        }

        private static InfisicalApiException BuildApiException(InfisicalHttpResponse response, InfisicalEndpointDefinition definition)
        {
            InfisicalApiException exception = new InfisicalApiException(string.Concat(
                "Infisical API returned ",
                response.StatusCode.ToString(CultureInfo.InvariantCulture),
                " (", response.ReasonPhrase ?? string.Empty, ")."));
            exception.StatusCode = response.StatusCode;
            exception.ReasonPhrase = response.ReasonPhrase;
            exception.EndpointName = definition.Name;
            exception.RequestMethod = definition.Method;
            exception.SanitizedBody = response.Body;
            return exception;
        }
    }
}
