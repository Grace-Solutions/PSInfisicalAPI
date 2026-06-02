using System;
using System.Collections.Generic;
using System.Security;
using PSInfisicalAPI.Endpoints;
using PSInfisicalAPI.Errors;
using PSInfisicalAPI.Http;
using PSInfisicalAPI.Logging;
using PSInfisicalAPI.Security;
using PSInfisicalAPI.Serialization;

namespace PSInfisicalAPI.Authentication
{
    public sealed class UniversalAuthProvider : IInfisicalAuthProvider
    {
        private const string Component = "UniversalAuthProvider";

        public string Name { get { return "UniversalAuth"; } }

        public InfisicalAuthenticationResult Authenticate(InfisicalAuthenticationRequest request, IInfisicalHttpClient httpClient, IInfisicalLogger logger)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (httpClient == null)
            {
                throw new ArgumentNullException(nameof(httpClient));
            }

            IInfisicalLogger log = logger ?? NullInfisicalLogger.Instance;

            if (string.IsNullOrEmpty(request.ClientId))
            {
                throw new InfisicalAuthenticationException("ClientId is required for Universal Auth.");
            }

            if (request.ClientSecret == null)
            {
                throw new InfisicalAuthenticationException("ClientSecret is required for Universal Auth.");
            }

            log.Information(Component, "Attempting to authenticate to Infisical. Please Wait...");

            InfisicalEndpointDefinition definition = InfisicalEndpointRegistry.Get(InfisicalEndpointNames.UniversalAuthLogin);
            Uri uri = InfisicalUriBuilder.Build(request.BaseUri, definition, null, null);
            JsonInfisicalSerializer serializer = new JsonInfisicalSerializer();

            InfisicalAuthenticationResult result = SecureStringUtility.UsePlainText(request.ClientSecret, plainSecret =>
            {
                Dictionary<string, string> bodyObject = new Dictionary<string, string>
                {
                    { "clientId", request.ClientId },
                    { "clientSecret", plainSecret ?? string.Empty }
                };

                string body = serializer.Serialize(bodyObject);

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
                        log.Error(Component, "Infisical authentication failed.");
                        throw new InfisicalAuthenticationException(string.Concat("Universal Auth login returned status ", response.StatusCode.ToString(System.Globalization.CultureInfo.InvariantCulture), "."));
                    }

                    UniversalAuthLoginResponse parsed = serializer.Deserialize<UniversalAuthLoginResponse>(response.Body);

                    if (parsed == null || string.IsNullOrEmpty(parsed.AccessToken))
                    {
                        throw new InfisicalAuthenticationException("Universal Auth login response did not contain an access token.");
                    }

                    SecureString accessToken = SecureStringUtility.ToReadOnlySecureString(parsed.AccessToken);

                    DateTimeOffset? expiresAt = null;
                    if (parsed.ExpiresIn > 0)
                    {
                        expiresAt = DateTimeOffset.UtcNow.AddSeconds(parsed.ExpiresIn);
                    }

                    parsed.AccessToken = null;

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
            });

            log.Information(Component, "Infisical authentication was successful.");
            return result;
        }

        private sealed class UniversalAuthLoginResponse
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
