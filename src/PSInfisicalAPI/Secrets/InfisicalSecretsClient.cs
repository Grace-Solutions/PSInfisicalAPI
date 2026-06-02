using System;
using System.Collections.Generic;
using System.Globalization;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Endpoints;
using PSInfisicalAPI.Errors;
using PSInfisicalAPI.Http;
using PSInfisicalAPI.Logging;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Security;
using PSInfisicalAPI.Serialization;

namespace PSInfisicalAPI.Secrets
{
    public sealed class InfisicalSecretsClient
    {
        private const string Component = "SecretsClient";

        private readonly IInfisicalHttpClient _httpClient;
        private readonly IInfisicalLogger _logger;
        private readonly JsonInfisicalSerializer _serializer;

        public InfisicalSecretsClient(IInfisicalHttpClient httpClient, IInfisicalLogger logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? NullInfisicalLogger.Instance;
            _serializer = new JsonInfisicalSerializer();
        }

        public InfisicalSecret[] List(InfisicalConnection connection, InfisicalListSecretsQuery query)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            if (query == null) { throw new ArgumentNullException(nameof(query)); }

            InfisicalEndpointDefinition definition = InfisicalEndpointRegistry.Get(InfisicalEndpointNames.ListSecrets);

            List<KeyValuePair<string, string>> queryParameters = new List<KeyValuePair<string, string>>();
            AddIfNotNull(queryParameters, "workspaceId", FirstNonEmpty(query.ProjectId, connection.ProjectId));
            AddIfNotNull(queryParameters, "environment", FirstNonEmpty(query.Environment, connection.Environment));
            AddIfNotNull(queryParameters, "secretPath", FirstNonEmpty(query.SecretPath, connection.DefaultSecretPath, "/"));
            queryParameters.Add(new KeyValuePair<string, string>("recursive", query.Recursive ? "true" : "false"));
            if (query.IncludeImports.HasValue) { queryParameters.Add(new KeyValuePair<string, string>("include_imports", query.IncludeImports.Value ? "true" : "false")); }
            if (query.IncludePersonalOverrides) { queryParameters.Add(new KeyValuePair<string, string>("includePersonalOverrides", "true")); }
            if (query.ExpandSecretReferences.HasValue) { queryParameters.Add(new KeyValuePair<string, string>("expandSecretReferences", query.ExpandSecretReferences.Value ? "true" : "false")); }
            if (query.ViewSecretValue.HasValue) { queryParameters.Add(new KeyValuePair<string, string>("viewSecretValue", query.ViewSecretValue.Value ? "true" : "false")); }

            if (query.TagSlugs != null)
            {
                foreach (string tag in query.TagSlugs)
                {
                    AddIfNotNull(queryParameters, "tagSlugs", tag);
                }
            }

            if (query.MetadataFilter != null)
            {
                foreach (KeyValuePair<string, string> meta in query.MetadataFilter)
                {
                    AddIfNotNull(queryParameters, string.Concat("metadata[", meta.Key, "]"), meta.Value);
                }
            }

            Uri uri = InfisicalUriBuilder.Build(connection.BaseUri, definition, null, queryParameters);
            InfisicalHttpResponse response = ExecuteAuthorized(connection, definition, "RetrieveSecrets", uri, null);

            try
            {
                _logger.Information(Component, "Attempting to retrieve Infisical secrets. Please Wait...");
                EnsureSuccess(response, definition);

                InfisicalSecretListResponseDto dto = _serializer.Deserialize<InfisicalSecretListResponseDto>(response.Body);
                response.Clear();

                InfisicalSecret[] mapped = InfisicalSecretMapper.MapMany(dto != null ? dto.Secrets : null);
                _logger.Information(Component, "Infisical secrets retrieval was successful.");
                return mapped;
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical secrets retrieval failed.");
                throw;
            }
        }

        public InfisicalSecret Retrieve(InfisicalConnection connection, InfisicalRetrieveSecretQuery query)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            if (query == null) { throw new ArgumentNullException(nameof(query)); }
            if (string.IsNullOrEmpty(query.SecretName)) { throw new InfisicalConfigurationException("SecretName is required."); }

            InfisicalEndpointDefinition definition = InfisicalEndpointRegistry.Get(InfisicalEndpointNames.RetrieveSecret);

            Dictionary<string, string> pathParameters = new Dictionary<string, string> { { "secretName", query.SecretName } };

            List<KeyValuePair<string, string>> queryParameters = new List<KeyValuePair<string, string>>();
            AddIfNotNull(queryParameters, "workspaceId", FirstNonEmpty(query.ProjectId, connection.ProjectId));
            AddIfNotNull(queryParameters, "environment", FirstNonEmpty(query.Environment, connection.Environment));
            AddIfNotNull(queryParameters, "secretPath", FirstNonEmpty(query.SecretPath, connection.DefaultSecretPath, "/"));
            AddIfNotNull(queryParameters, "type", string.IsNullOrEmpty(query.Type) ? "shared" : query.Type.ToLowerInvariant());
            if (query.Version.HasValue) { queryParameters.Add(new KeyValuePair<string, string>("version", query.Version.Value.ToString(CultureInfo.InvariantCulture))); }
            if (query.ViewSecretValue.HasValue) { queryParameters.Add(new KeyValuePair<string, string>("viewSecretValue", query.ViewSecretValue.Value ? "true" : "false")); }
            if (query.ExpandSecretReferences.HasValue) { queryParameters.Add(new KeyValuePair<string, string>("expandSecretReferences", query.ExpandSecretReferences.Value ? "true" : "false")); }
            if (query.IncludeImports.HasValue) { queryParameters.Add(new KeyValuePair<string, string>("include_imports", query.IncludeImports.Value ? "true" : "false")); }

            Uri uri = InfisicalUriBuilder.Build(connection.BaseUri, definition, pathParameters, queryParameters);
            InfisicalHttpResponse response = ExecuteAuthorized(connection, definition, "RetrieveSecret", uri, null);

            try
            {
                _logger.Information(Component, string.Concat("Attempting to retrieve Infisical secret '", query.SecretName, "'. Please Wait..."));
                EnsureSuccess(response, definition);

                InfisicalSecretSingleResponseDto dto = _serializer.Deserialize<InfisicalSecretSingleResponseDto>(response.Body);
                response.Clear();

                InfisicalSecret mapped = InfisicalSecretMapper.Map(dto != null ? dto.Secret : null);
                _logger.Information(Component, "Infisical secret retrieval was successful.");
                return mapped;
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical secret retrieval failed.");
                throw;
            }
        }

        private InfisicalHttpResponse ExecuteAuthorized(InfisicalConnection connection, InfisicalEndpointDefinition definition, string operationName, Uri uri, string body)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            headers["Accept"] = "application/json";

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

        private static void EnsureSuccess(InfisicalHttpResponse response, InfisicalEndpointDefinition definition)
        {
            if (response.StatusCode >= 200 && response.StatusCode < 300)
            {
                return;
            }

            InfisicalApiException exception = new InfisicalApiException(string.Concat("Infisical API returned ", response.StatusCode.ToString(CultureInfo.InvariantCulture), " (", response.ReasonPhrase ?? string.Empty, ")."));
            exception.StatusCode = response.StatusCode;
            exception.ReasonPhrase = response.ReasonPhrase;
            exception.EndpointName = definition.Name;
            exception.RequestMethod = definition.Method;
            exception.SanitizedBody = definition.ContainsSecretMaterialInResponse ? "[REDACTED]" : response.Body;
            response.Clear();
            throw exception;
        }

        private static void AddIfNotNull(List<KeyValuePair<string, string>> list, string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                list.Add(new KeyValuePair<string, string>(key, value));
            }
        }

        private static string FirstNonEmpty(params string[] values)
        {
            if (values == null) { return null; }
            foreach (string value in values)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    return value;
                }
            }

            return null;
        }
    }
}
