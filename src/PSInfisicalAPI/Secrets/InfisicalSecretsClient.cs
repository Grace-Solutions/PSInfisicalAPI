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

            string resolvedProjectId = FirstNonEmpty(query.ProjectId, connection.ProjectId);

            List<KeyValuePair<string, string>> queryParameters = new List<KeyValuePair<string, string>>();
            AddIfNotNull(queryParameters, "workspaceId", resolvedProjectId);
            AddIfNotNull(queryParameters, "projectId", resolvedProjectId);
            AddIfNotNull(queryParameters, "environment", FirstNonEmpty(query.Environment, connection.Environment));
            AddIfNotNull(queryParameters, "secretPath", FirstNonEmpty(query.SecretPath, connection.DefaultSecretPath, "/"));
            queryParameters.Add(new KeyValuePair<string, string>("recursive", query.Recursive ? "true" : "false"));
            if (query.IncludeImports.HasValue) { queryParameters.Add(new KeyValuePair<string, string>("includeImports", query.IncludeImports.Value ? "true" : "false")); }
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

            try
            {
                _logger.Information(Component, "Attempting to retrieve Infisical secrets. Please Wait...");

                InfisicalHttpResponse response = SendWithVersionFallback(
                    connection,
                    InfisicalEndpointNames.ListSecrets,
                    query.ApiVersion,
                    "RetrieveSecrets",
                    null,
                    queryParameters,
                    null);

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

            Dictionary<string, string> pathParameters = new Dictionary<string, string> { { "secretName", query.SecretName } };

            string resolvedProjectId = FirstNonEmpty(query.ProjectId, connection.ProjectId);

            List<KeyValuePair<string, string>> queryParameters = new List<KeyValuePair<string, string>>();
            AddIfNotNull(queryParameters, "workspaceId", resolvedProjectId);
            AddIfNotNull(queryParameters, "projectId", resolvedProjectId);
            AddIfNotNull(queryParameters, "environment", FirstNonEmpty(query.Environment, connection.Environment));
            AddIfNotNull(queryParameters, "secretPath", FirstNonEmpty(query.SecretPath, connection.DefaultSecretPath, "/"));
            AddIfNotNull(queryParameters, "type", string.IsNullOrEmpty(query.Type) ? "shared" : query.Type.ToLowerInvariant());
            if (query.Version.HasValue) { queryParameters.Add(new KeyValuePair<string, string>("version", query.Version.Value.ToString(CultureInfo.InvariantCulture))); }
            if (query.ViewSecretValue.HasValue) { queryParameters.Add(new KeyValuePair<string, string>("viewSecretValue", query.ViewSecretValue.Value ? "true" : "false")); }
            if (query.ExpandSecretReferences.HasValue) { queryParameters.Add(new KeyValuePair<string, string>("expandSecretReferences", query.ExpandSecretReferences.Value ? "true" : "false")); }
            if (query.IncludeImports.HasValue) { queryParameters.Add(new KeyValuePair<string, string>("includeImports", query.IncludeImports.Value ? "true" : "false")); }

            try
            {
                _logger.Information(Component, string.Concat("Attempting to retrieve Infisical secret '", query.SecretName, "'. Please Wait..."));

                InfisicalHttpResponse response = SendWithVersionFallback(
                    connection,
                    InfisicalEndpointNames.RetrieveSecret,
                    query.ApiVersion,
                    "RetrieveSecret",
                    pathParameters,
                    queryParameters,
                    null);

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

        public InfisicalSecret Create(InfisicalConnection connection, InfisicalCreateSecretRequest request)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            if (request == null) { throw new ArgumentNullException(nameof(request)); }
            if (string.IsNullOrEmpty(request.SecretName)) { throw new InfisicalConfigurationException("SecretName is required."); }
            if (request.SecretValue == null) { throw new InfisicalConfigurationException("SecretValue is required."); }

            string resolvedProjectId = FirstNonEmpty(request.ProjectId, connection.ProjectId);
            string resolvedEnvironment = FirstNonEmpty(request.Environment, connection.Environment);
            if (string.IsNullOrEmpty(resolvedProjectId)) { throw new InfisicalConfigurationException("ProjectId is required."); }
            if (string.IsNullOrEmpty(resolvedEnvironment)) { throw new InfisicalConfigurationException("Environment is required."); }

            Dictionary<string, string> pathParameters = new Dictionary<string, string> { { "secretName", request.SecretName } };
            InfisicalSecretCreateRequestDto dtoRequest = new InfisicalSecretCreateRequestDto
            {
                WorkspaceId = resolvedProjectId,
                Environment = resolvedEnvironment,
                SecretPath = FirstNonEmpty(request.SecretPath, connection.DefaultSecretPath, "/"),
                Type = string.IsNullOrEmpty(request.Type) ? "shared" : request.Type.ToLowerInvariant(),
                SecretValue = request.SecretValue,
                SecretComment = request.SecretComment,
                SkipMultilineEncoding = request.SkipMultilineEncoding,
                TagIds = request.TagIds
            };
            string body = _serializer.Serialize(dtoRequest);

            try
            {
                _logger.Information(Component, string.Concat("Attempting to create Infisical secret '", request.SecretName, "'. Please Wait..."));
                InfisicalHttpResponse response = SendWithVersionFallback(connection, InfisicalEndpointNames.CreateSecret, request.ApiVersion, "CreateSecret", pathParameters, null, body);
                InfisicalSecretSingleResponseDto dto = _serializer.Deserialize<InfisicalSecretSingleResponseDto>(response.Body);
                response.Clear();

                InfisicalSecret mapped = InfisicalSecretMapper.Map(dto != null ? dto.Secret : null);
                _logger.Information(Component, "Infisical secret creation was successful.");
                return mapped;
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical secret creation failed.");
                throw;
            }
        }

        public InfisicalSecret Update(InfisicalConnection connection, InfisicalUpdateSecretRequest request)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            if (request == null) { throw new ArgumentNullException(nameof(request)); }
            if (string.IsNullOrEmpty(request.SecretName)) { throw new InfisicalConfigurationException("SecretName is required."); }

            string resolvedProjectId = FirstNonEmpty(request.ProjectId, connection.ProjectId);
            string resolvedEnvironment = FirstNonEmpty(request.Environment, connection.Environment);
            if (string.IsNullOrEmpty(resolvedProjectId)) { throw new InfisicalConfigurationException("ProjectId is required."); }
            if (string.IsNullOrEmpty(resolvedEnvironment)) { throw new InfisicalConfigurationException("Environment is required."); }

            Dictionary<string, string> pathParameters = new Dictionary<string, string> { { "secretName", request.SecretName } };
            InfisicalSecretUpdateRequestDto dtoRequest = new InfisicalSecretUpdateRequestDto
            {
                WorkspaceId = resolvedProjectId,
                Environment = resolvedEnvironment,
                SecretPath = FirstNonEmpty(request.SecretPath, connection.DefaultSecretPath, "/"),
                Type = string.IsNullOrEmpty(request.Type) ? "shared" : request.Type.ToLowerInvariant(),
                SecretValue = request.SecretValue,
                SecretComment = request.SecretComment,
                NewSecretName = request.NewSecretName,
                SkipMultilineEncoding = request.SkipMultilineEncoding,
                TagIds = request.TagIds
            };
            string body = _serializer.Serialize(dtoRequest);

            try
            {
                _logger.Information(Component, string.Concat("Attempting to update Infisical secret '", request.SecretName, "'. Please Wait..."));
                InfisicalHttpResponse response = SendWithVersionFallback(connection, InfisicalEndpointNames.UpdateSecret, request.ApiVersion, "UpdateSecret", pathParameters, null, body);
                InfisicalSecretSingleResponseDto dto = _serializer.Deserialize<InfisicalSecretSingleResponseDto>(response.Body);
                response.Clear();

                InfisicalSecret mapped = InfisicalSecretMapper.Map(dto != null ? dto.Secret : null);
                _logger.Information(Component, "Infisical secret update was successful.");
                return mapped;
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical secret update failed.");
                throw;
            }
        }

        public InfisicalSecret[] CreateBatch(InfisicalConnection connection, InfisicalBulkCreateSecretsRequest request)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            if (request == null) { throw new ArgumentNullException(nameof(request)); }
            if (request.Secrets == null || request.Secrets.Length == 0) { throw new InfisicalConfigurationException("At least one secret is required."); }

            string resolvedProjectId = FirstNonEmpty(request.ProjectId, connection.ProjectId);
            string resolvedEnvironment = FirstNonEmpty(request.Environment, connection.Environment);
            if (string.IsNullOrEmpty(resolvedProjectId)) { throw new InfisicalConfigurationException("ProjectId is required."); }
            if (string.IsNullOrEmpty(resolvedEnvironment)) { throw new InfisicalConfigurationException("Environment is required."); }

            List<InfisicalSecretBatchCreateItemDto> items = new List<InfisicalSecretBatchCreateItemDto>(request.Secrets.Length);
            foreach (InfisicalBulkCreateSecretItem item in request.Secrets)
            {
                if (item == null) { continue; }
                if (string.IsNullOrEmpty(item.SecretName)) { throw new InfisicalConfigurationException("Each bulk-create item requires SecretName."); }
                items.Add(new InfisicalSecretBatchCreateItemDto
                {
                    SecretKey = item.SecretName,
                    SecretValue = item.SecretValue ?? string.Empty,
                    SecretComment = item.SecretComment,
                    SkipMultilineEncoding = item.SkipMultilineEncoding,
                    TagIds = item.TagIds,
                    SecretMetadata = ToMetadataDtoList(item.SecretMetadata)
                });
            }

            InfisicalSecretBatchCreateRequestDto dtoRequest = new InfisicalSecretBatchCreateRequestDto
            {
                WorkspaceId = resolvedProjectId,
                ProjectId = resolvedProjectId,
                Environment = resolvedEnvironment,
                SecretPath = FirstNonEmpty(request.SecretPath, connection.DefaultSecretPath, "/"),
                Secrets = items
            };
            string body = _serializer.Serialize(dtoRequest);

            try
            {
                _logger.Information(Component, string.Concat("Attempting to bulk-create ", items.Count.ToString(CultureInfo.InvariantCulture), " Infisical secret(s). Please Wait..."));
                InfisicalHttpResponse response = SendWithVersionFallback(connection, InfisicalEndpointNames.BulkCreateSecret, request.ApiVersion, "BulkCreateSecrets", null, null, body);
                InfisicalSecretListResponseDto dto = _serializer.Deserialize<InfisicalSecretListResponseDto>(response.Body);
                response.Clear();

                InfisicalSecret[] mapped = InfisicalSecretMapper.MapMany(dto != null ? dto.Secrets : null);
                _logger.Information(Component, "Infisical bulk secret creation was successful.");
                return mapped;
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical bulk secret creation failed.");
                throw;
            }
        }

        public InfisicalSecret[] UpdateBatch(InfisicalConnection connection, InfisicalBulkUpdateSecretsRequest request)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            if (request == null) { throw new ArgumentNullException(nameof(request)); }
            if (request.Secrets == null || request.Secrets.Length == 0) { throw new InfisicalConfigurationException("At least one secret is required."); }

            string resolvedProjectId = FirstNonEmpty(request.ProjectId, connection.ProjectId);
            string resolvedEnvironment = FirstNonEmpty(request.Environment, connection.Environment);
            if (string.IsNullOrEmpty(resolvedProjectId)) { throw new InfisicalConfigurationException("ProjectId is required."); }
            if (string.IsNullOrEmpty(resolvedEnvironment)) { throw new InfisicalConfigurationException("Environment is required."); }

            List<InfisicalSecretBatchUpdateItemDto> items = new List<InfisicalSecretBatchUpdateItemDto>(request.Secrets.Length);
            foreach (InfisicalBulkUpdateSecretItem item in request.Secrets)
            {
                if (item == null) { continue; }
                if (string.IsNullOrEmpty(item.SecretName)) { throw new InfisicalConfigurationException("Each bulk-update item requires SecretName."); }
                items.Add(new InfisicalSecretBatchUpdateItemDto
                {
                    SecretKey = item.SecretName,
                    NewSecretName = item.NewSecretName,
                    SecretValue = item.SecretValue,
                    SecretComment = item.SecretComment,
                    SkipMultilineEncoding = item.SkipMultilineEncoding,
                    TagIds = item.TagIds,
                    SecretMetadata = ToMetadataDtoList(item.SecretMetadata)
                });
            }

            InfisicalSecretBatchUpdateRequestDto dtoRequest = new InfisicalSecretBatchUpdateRequestDto
            {
                WorkspaceId = resolvedProjectId,
                ProjectId = resolvedProjectId,
                Environment = resolvedEnvironment,
                SecretPath = FirstNonEmpty(request.SecretPath, connection.DefaultSecretPath, "/"),
                Mode = request.Mode,
                Secrets = items
            };
            string body = _serializer.Serialize(dtoRequest);

            try
            {
                _logger.Information(Component, string.Concat("Attempting to bulk-update ", items.Count.ToString(CultureInfo.InvariantCulture), " Infisical secret(s). Please Wait..."));
                InfisicalHttpResponse response = SendWithVersionFallback(connection, InfisicalEndpointNames.BulkUpdateSecret, request.ApiVersion, "BulkUpdateSecrets", null, null, body);
                InfisicalSecretListResponseDto dto = _serializer.Deserialize<InfisicalSecretListResponseDto>(response.Body);
                response.Clear();

                InfisicalSecret[] mapped = InfisicalSecretMapper.MapMany(dto != null ? dto.Secrets : null);
                _logger.Information(Component, "Infisical bulk secret update was successful.");
                return mapped;
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical bulk secret update failed.");
                throw;
            }
        }

        public void DeleteBatch(InfisicalConnection connection, InfisicalBulkDeleteSecretsRequest request)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            if (request == null) { throw new ArgumentNullException(nameof(request)); }
            if (request.SecretNames == null || request.SecretNames.Length == 0) { throw new InfisicalConfigurationException("At least one secret name is required."); }

            string resolvedProjectId = FirstNonEmpty(request.ProjectId, connection.ProjectId);
            string resolvedEnvironment = FirstNonEmpty(request.Environment, connection.Environment);
            if (string.IsNullOrEmpty(resolvedProjectId)) { throw new InfisicalConfigurationException("ProjectId is required."); }
            if (string.IsNullOrEmpty(resolvedEnvironment)) { throw new InfisicalConfigurationException("Environment is required."); }

            List<InfisicalSecretBatchDeleteItemDto> items = new List<InfisicalSecretBatchDeleteItemDto>(request.SecretNames.Length);
            foreach (string name in request.SecretNames)
            {
                if (string.IsNullOrEmpty(name)) { continue; }
                items.Add(new InfisicalSecretBatchDeleteItemDto { SecretKey = name });
            }

            InfisicalSecretBatchDeleteRequestDto dtoRequest = new InfisicalSecretBatchDeleteRequestDto
            {
                WorkspaceId = resolvedProjectId,
                ProjectId = resolvedProjectId,
                Environment = resolvedEnvironment,
                SecretPath = FirstNonEmpty(request.SecretPath, connection.DefaultSecretPath, "/"),
                Secrets = items
            };
            string body = _serializer.Serialize(dtoRequest);

            try
            {
                _logger.Information(Component, string.Concat("Attempting to bulk-delete ", items.Count.ToString(CultureInfo.InvariantCulture), " Infisical secret(s). Please Wait..."));
                InfisicalHttpResponse response = SendWithVersionFallback(connection, InfisicalEndpointNames.BulkDeleteSecret, request.ApiVersion, "BulkDeleteSecrets", null, null, body);
                response.Clear();
                _logger.Information(Component, "Infisical bulk secret deletion was successful.");
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical bulk secret deletion failed.");
                throw;
            }
        }

        public InfisicalSecret[] Duplicate(InfisicalConnection connection, InfisicalDuplicateSecretsRequest request)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            if (request == null) { throw new ArgumentNullException(nameof(request)); }
            if (request.SecretIds == null || request.SecretIds.Length == 0) { throw new InfisicalConfigurationException("At least one SecretId is required."); }

            string resolvedProjectId = FirstNonEmpty(request.ProjectId, connection.ProjectId);
            string resolvedSourceEnv = FirstNonEmpty(request.SourceEnvironment, connection.Environment);
            if (string.IsNullOrEmpty(resolvedProjectId)) { throw new InfisicalConfigurationException("ProjectId is required."); }
            if (string.IsNullOrEmpty(resolvedSourceEnv)) { throw new InfisicalConfigurationException("SourceEnvironment is required."); }
            if (string.IsNullOrEmpty(request.DestinationEnvironment)) { throw new InfisicalConfigurationException("DestinationEnvironment is required."); }

            string resolvedSourcePath = FirstNonEmpty(request.SourceSecretPath, connection.DefaultSecretPath, "/");
            string resolvedDestPath = FirstNonEmpty(request.DestinationSecretPath, resolvedSourcePath);

            InfisicalSecretDuplicateAttributesDto attributes = null;
            if (request.CopySecretValue.HasValue || request.CopySecretComment.HasValue || request.CopyTags.HasValue || request.CopyMetadata.HasValue)
            {
                attributes = new InfisicalSecretDuplicateAttributesDto
                {
                    SecretValue = request.CopySecretValue,
                    SecretComment = request.CopySecretComment,
                    Tags = request.CopyTags,
                    Metadata = request.CopyMetadata
                };
            }

            InfisicalSecretDuplicateRequestDto dtoRequest = new InfisicalSecretDuplicateRequestDto
            {
                ProjectId = resolvedProjectId,
                SourceEnvironment = resolvedSourceEnv,
                DestinationEnvironment = request.DestinationEnvironment,
                SourceSecretPath = resolvedSourcePath,
                DestinationSecretPath = resolvedDestPath,
                SecretIds = request.SecretIds,
                OverwriteExisting = request.OverwriteExisting,
                AttributesToCopy = attributes
            };
            string body = _serializer.Serialize(dtoRequest);

            try
            {
                _logger.Information(Component, string.Concat("Attempting to duplicate ", request.SecretIds.Length.ToString(CultureInfo.InvariantCulture), " Infisical secret(s). Please Wait..."));
                InfisicalHttpResponse response = SendWithVersionFallback(connection, InfisicalEndpointNames.DuplicateSecret, request.ApiVersion, "DuplicateSecrets", null, null, body);
                InfisicalSecretListResponseDto dto = _serializer.Deserialize<InfisicalSecretListResponseDto>(response.Body);
                response.Clear();

                InfisicalSecret[] mapped = InfisicalSecretMapper.MapMany(dto != null ? dto.Secrets : null);
                _logger.Information(Component, "Infisical secret duplication was successful.");
                return mapped;
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical secret duplication failed.");
                throw;
            }
        }

        private static List<InfisicalSecretMetadataDto> ToMetadataDtoList(Dictionary<string, string> metadata)
        {
            if (metadata == null || metadata.Count == 0) { return null; }
            List<InfisicalSecretMetadataDto> list = new List<InfisicalSecretMetadataDto>(metadata.Count);
            foreach (KeyValuePair<string, string> kvp in metadata)
            {
                list.Add(new InfisicalSecretMetadataDto { Key = kvp.Key, Value = kvp.Value });
            }

            return list;
        }

        public void Delete(InfisicalConnection connection, InfisicalDeleteSecretRequest request)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            if (request == null) { throw new ArgumentNullException(nameof(request)); }
            if (string.IsNullOrEmpty(request.SecretName)) { throw new InfisicalConfigurationException("SecretName is required."); }

            string resolvedProjectId = FirstNonEmpty(request.ProjectId, connection.ProjectId);
            string resolvedEnvironment = FirstNonEmpty(request.Environment, connection.Environment);
            if (string.IsNullOrEmpty(resolvedProjectId)) { throw new InfisicalConfigurationException("ProjectId is required."); }
            if (string.IsNullOrEmpty(resolvedEnvironment)) { throw new InfisicalConfigurationException("Environment is required."); }

            Dictionary<string, string> pathParameters = new Dictionary<string, string> { { "secretName", request.SecretName } };
            InfisicalSecretDeleteRequestDto dtoRequest = new InfisicalSecretDeleteRequestDto
            {
                WorkspaceId = resolvedProjectId,
                Environment = resolvedEnvironment,
                SecretPath = FirstNonEmpty(request.SecretPath, connection.DefaultSecretPath, "/"),
                Type = string.IsNullOrEmpty(request.Type) ? "shared" : request.Type.ToLowerInvariant()
            };
            string body = _serializer.Serialize(dtoRequest);

            try
            {
                _logger.Information(Component, string.Concat("Attempting to delete Infisical secret '", request.SecretName, "'. Please Wait..."));
                InfisicalHttpResponse response = SendWithVersionFallback(connection, InfisicalEndpointNames.DeleteSecret, request.ApiVersion, "DeleteSecret", pathParameters, null, body);
                response.Clear();
                _logger.Information(Component, "Infisical secret deletion was successful.");
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical secret deletion failed.");
                throw;
            }
        }

        private InfisicalHttpResponse SendWithVersionFallback(
            InfisicalConnection connection,
            string endpointName,
            string perCallApiVersion,
            string operationName,
            Dictionary<string, string> pathParameters,
            List<KeyValuePair<string, string>> queryParameters,
            string body)
        {
            IReadOnlyList<InfisicalEndpointDefinition> allCandidates = InfisicalEndpointRegistry.GetCandidates(endpointName);

            string pinned = FirstNonEmpty(perCallApiVersion, connection.PinnedApiVersion);
            string cached;
            connection.ResolvedEndpointVersions.TryGetValue(endpointName, out cached);

            List<InfisicalEndpointDefinition> candidates = OrderCandidates(allCandidates, pinned, cached);

            if (candidates.Count == 0)
            {
                throw new InfisicalConfigurationException(string.Concat(
                    "No matching endpoint candidate for '", endpointName,
                    "' with ApiVersion='", pinned ?? string.Empty, "'."));
            }

            InfisicalApiException lastException = null;

            for (int index = 0; index < candidates.Count; index++)
            {
                InfisicalEndpointDefinition definition = candidates[index];
                Uri uri = InfisicalUriBuilder.Build(connection.BaseUri, definition, pathParameters, queryParameters);
                InfisicalHttpResponse response = ExecuteAuthorized(connection, definition, operationName, uri, body);

                if (response.StatusCode >= 200 && response.StatusCode < 300)
                {
                    connection.ResolvedEndpointVersions[endpointName] = definition.Version;
                    return response;
                }

                InfisicalApiException exception = BuildApiException(response, definition);
                response.Clear();

                bool hasMoreCandidates = (index + 1) < candidates.Count;
                bool pinnedHere = !string.IsNullOrEmpty(pinned);

                if (!pinnedHere && hasMoreCandidates && IsVersionMismatch(exception))
                {
                    _logger.Warning(Component, string.Concat(
                        "Endpoint '", endpointName, "' version '", definition.Version,
                        "' rejected by server (", exception.StatusCode.ToString(CultureInfo.InvariantCulture),
                        "); falling back to next candidate."));
                    lastException = exception;
                    continue;
                }

                throw exception;
            }

            throw lastException ?? new InfisicalApiException(string.Concat(
                "All API version candidates exhausted for '", endpointName, "'."));
        }

        private static List<InfisicalEndpointDefinition> OrderCandidates(
            IReadOnlyList<InfisicalEndpointDefinition> allCandidates,
            string pinned,
            string cached)
        {
            List<InfisicalEndpointDefinition> ordered = new List<InfisicalEndpointDefinition>();

            if (!string.IsNullOrEmpty(pinned))
            {
                foreach (InfisicalEndpointDefinition candidate in allCandidates)
                {
                    if (string.Equals(candidate.Version, pinned, StringComparison.OrdinalIgnoreCase))
                    {
                        ordered.Add(candidate);
                    }
                }

                return ordered;
            }

            if (!string.IsNullOrEmpty(cached))
            {
                foreach (InfisicalEndpointDefinition candidate in allCandidates)
                {
                    if (string.Equals(candidate.Version, cached, StringComparison.OrdinalIgnoreCase))
                    {
                        ordered.Add(candidate);
                        break;
                    }
                }

                foreach (InfisicalEndpointDefinition candidate in allCandidates)
                {
                    if (!string.Equals(candidate.Version, cached, StringComparison.OrdinalIgnoreCase))
                    {
                        ordered.Add(candidate);
                    }
                }

                return ordered;
            }

            foreach (InfisicalEndpointDefinition candidate in allCandidates)
            {
                ordered.Add(candidate);
            }

            return ordered;
        }

        private static bool IsVersionMismatch(InfisicalApiException exception)
        {
            string body = exception.SanitizedBody;
            bool hasInfisicalErrorEnvelope = !string.IsNullOrEmpty(body)
                && body.IndexOf("\"reqId\"", StringComparison.OrdinalIgnoreCase) >= 0
                && body.IndexOf("\"error\"", StringComparison.OrdinalIgnoreCase) >= 0;

            if (exception.StatusCode == 405)
            {
                return true;
            }

            if (exception.StatusCode == 404 && !hasInfisicalErrorEnvelope)
            {
                return true;
            }

            if (exception.StatusCode == 400 && !string.IsNullOrEmpty(body))
            {
                if (body.IndexOf("projectSlug", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    body.IndexOf("workspaceId", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
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
