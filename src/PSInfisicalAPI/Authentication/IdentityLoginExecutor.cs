using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security;
using PSInfisicalAPI.Endpoints;
using PSInfisicalAPI.Errors;
using PSInfisicalAPI.Http;
using PSInfisicalAPI.Logging;
using PSInfisicalAPI.Security;
using PSInfisicalAPI.Serialization;

namespace PSInfisicalAPI.Authentication
{
    internal static class IdentityLoginExecutor
    {
        internal static InfisicalAuthenticationResult Execute(
            string endpointName,
            string component,
            InfisicalAuthenticationRequest request,
            IInfisicalHttpClient httpClient,
            IInfisicalLogger logger,
            Func<JsonInfisicalSerializer, string> bodyFactory)
        {
            if (request == null) { throw new ArgumentNullException(nameof(request)); }
            if (httpClient == null) { throw new ArgumentNullException(nameof(httpClient)); }
            if (bodyFactory == null) { throw new ArgumentNullException(nameof(bodyFactory)); }

            IInfisicalLogger log = logger ?? NullInfisicalLogger.Instance;
            log.Information(component, "Attempting to authenticate to Infisical. Please Wait...");

            InfisicalEndpointDefinition definition = InfisicalEndpointRegistry.Get(endpointName);
            Uri uri = InfisicalUriBuilder.Build(request.BaseUri, definition, null, null);
            JsonInfisicalSerializer serializer = new JsonInfisicalSerializer();
            string body = bodyFactory(serializer);

            InfisicalHttpRequest httpRequest = new InfisicalHttpRequest
            {
                OperationName = "Authenticate",
                EndpointName = definition.Name,
                Method = definition.Method,
                Uri = uri,
                Body = body,
                ContentType = "application/json",
                ContainsSecretMaterialInRequest = definition.ContainsSecretMaterialInRequest,
                ContainsSecretMaterialInResponse = definition.ContainsSecretMaterialInResponse,
                Headers = new Dictionary<string, string> { { "Accept", "application/json" } }
            };

            InfisicalHttpResponse response = httpClient.Send(httpRequest);

            try
            {
                if (response.StatusCode < 200 || response.StatusCode >= 300)
                {
                    log.Error(component, "Infisical authentication failed.");
                    throw new InfisicalAuthenticationException(string.Concat(component, " login returned status ", response.StatusCode.ToString(CultureInfo.InvariantCulture), "."));
                }

                IdentityLoginResponse parsed = serializer.Deserialize<IdentityLoginResponse>(response.Body);
                if (parsed == null || string.IsNullOrEmpty(parsed.AccessToken))
                {
                    throw new InfisicalAuthenticationException(string.Concat(component, " login response did not contain an access token."));
                }

                SecureString accessToken = SecureStringUtility.ToReadOnlySecureString(parsed.AccessToken);

                DateTimeOffset? expiresAt = null;
                if (parsed.ExpiresIn > 0)
                {
                    expiresAt = DateTimeOffset.UtcNow.AddSeconds(parsed.ExpiresIn);
                }

                parsed.AccessToken = null;

                log.Information(component, "Infisical authentication was successful.");
                return new InfisicalAuthenticationResult
                {
                    AccessToken = accessToken,
                    TokenType = string.IsNullOrEmpty(parsed.TokenType) ? "Bearer" : parsed.TokenType,
                    ExpiresAtUtc = expiresAt
                };
            }
            finally
            {
                response.Clear();
            }
        }

        private sealed class IdentityLoginResponse
        {
            [Newtonsoft.Json.JsonProperty("accessToken")]
            public string AccessToken { get; set; }

            [Newtonsoft.Json.JsonProperty("expiresIn")]
            public int ExpiresIn { get; set; }

            [Newtonsoft.Json.JsonProperty("tokenType")]
            public string TokenType { get; set; }
        }
    }
}
