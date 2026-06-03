using System.Collections.Generic;
using Newtonsoft.Json;

namespace PSInfisicalAPI.Secrets
{
    internal sealed class InfisicalSecretResponseDto
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("_id")] public string InternalId { get; set; }
        [JsonProperty("workspace")] public string Workspace { get; set; }
        [JsonProperty("environment")] public string Environment { get; set; }
        [JsonProperty("version")] public int? Version { get; set; }
        [JsonProperty("type")] public string Type { get; set; }
        [JsonProperty("secretKey")] public string SecretKey { get; set; }
        [JsonProperty("secretValue")] public string SecretValue { get; set; }
        [JsonProperty("secretValueHidden")] public bool SecretValueHidden { get; set; }
        [JsonProperty("secretPath")] public string SecretPath { get; set; }
        [JsonProperty("secretComment")] public string SecretComment { get; set; }
        [JsonProperty("createdAt")] public string CreatedAt { get; set; }
        [JsonProperty("updatedAt")] public string UpdatedAt { get; set; }
        [JsonProperty("isRotatedSecret")] public bool IsRotatedSecret { get; set; }
        [JsonProperty("rotationId")] public string RotationId { get; set; }
        [JsonProperty("tags")] public List<InfisicalSecretTagDto> Tags { get; set; }
        [JsonProperty("secretMetadata")] public List<InfisicalSecretMetadataDto> SecretMetadata { get; set; }
    }

    internal sealed class InfisicalSecretTagDto
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("slug")] public string Slug { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("color")] public string Color { get; set; }
    }

    internal sealed class InfisicalSecretMetadataDto
    {
        [JsonProperty("key")] public string Key { get; set; }
        [JsonProperty("value")] public string Value { get; set; }
    }

    internal sealed class InfisicalSecretListResponseDto
    {
        [JsonProperty("secrets")] public List<InfisicalSecretResponseDto> Secrets { get; set; }
        [JsonProperty("imports")] public List<InfisicalSecretImportDto> Imports { get; set; }
    }

    internal sealed class InfisicalSecretImportDto
    {
        [JsonProperty("secretPath")] public string SecretPath { get; set; }
        [JsonProperty("environment")] public string Environment { get; set; }
        [JsonProperty("secrets")] public List<InfisicalSecretResponseDto> Secrets { get; set; }
    }

    internal sealed class InfisicalSecretSingleResponseDto
    {
        [JsonProperty("secret")] public InfisicalSecretResponseDto Secret { get; set; }
    }

    internal sealed class InfisicalSecretCreateRequestDto
    {
        [JsonProperty("workspaceId")] public string WorkspaceId { get; set; }
        [JsonProperty("environment")] public string Environment { get; set; }
        [JsonProperty("secretPath", NullValueHandling = NullValueHandling.Ignore)] public string SecretPath { get; set; }
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)] public string Type { get; set; }
        [JsonProperty("secretValue")] public string SecretValue { get; set; }
        [JsonProperty("secretComment", NullValueHandling = NullValueHandling.Ignore)] public string SecretComment { get; set; }
        [JsonProperty("skipMultilineEncoding", NullValueHandling = NullValueHandling.Ignore)] public bool? SkipMultilineEncoding { get; set; }
        [JsonProperty("tagIds", NullValueHandling = NullValueHandling.Ignore)] public string[] TagIds { get; set; }
    }

    internal sealed class InfisicalSecretUpdateRequestDto
    {
        [JsonProperty("workspaceId")] public string WorkspaceId { get; set; }
        [JsonProperty("environment")] public string Environment { get; set; }
        [JsonProperty("secretPath", NullValueHandling = NullValueHandling.Ignore)] public string SecretPath { get; set; }
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)] public string Type { get; set; }
        [JsonProperty("secretValue", NullValueHandling = NullValueHandling.Ignore)] public string SecretValue { get; set; }
        [JsonProperty("secretComment", NullValueHandling = NullValueHandling.Ignore)] public string SecretComment { get; set; }
        [JsonProperty("newSecretName", NullValueHandling = NullValueHandling.Ignore)] public string NewSecretName { get; set; }
        [JsonProperty("skipMultilineEncoding", NullValueHandling = NullValueHandling.Ignore)] public bool? SkipMultilineEncoding { get; set; }
        [JsonProperty("tagIds", NullValueHandling = NullValueHandling.Ignore)] public string[] TagIds { get; set; }
    }

    internal sealed class InfisicalSecretDeleteRequestDto
    {
        [JsonProperty("workspaceId")] public string WorkspaceId { get; set; }
        [JsonProperty("environment")] public string Environment { get; set; }
        [JsonProperty("secretPath", NullValueHandling = NullValueHandling.Ignore)] public string SecretPath { get; set; }
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)] public string Type { get; set; }
    }

    internal sealed class InfisicalSecretBatchCreateItemDto
    {
        [JsonProperty("secretKey")] public string SecretKey { get; set; }
        [JsonProperty("secretValue")] public string SecretValue { get; set; }
        [JsonProperty("secretComment", NullValueHandling = NullValueHandling.Ignore)] public string SecretComment { get; set; }
        [JsonProperty("skipMultilineEncoding", NullValueHandling = NullValueHandling.Ignore)] public bool? SkipMultilineEncoding { get; set; }
        [JsonProperty("tagIds", NullValueHandling = NullValueHandling.Ignore)] public string[] TagIds { get; set; }
        [JsonProperty("secretMetadata", NullValueHandling = NullValueHandling.Ignore)] public List<InfisicalSecretMetadataDto> SecretMetadata { get; set; }
    }

    internal sealed class InfisicalSecretBatchUpdateItemDto
    {
        [JsonProperty("secretKey")] public string SecretKey { get; set; }
        [JsonProperty("newSecretName", NullValueHandling = NullValueHandling.Ignore)] public string NewSecretName { get; set; }
        [JsonProperty("secretValue", NullValueHandling = NullValueHandling.Ignore)] public string SecretValue { get; set; }
        [JsonProperty("secretComment", NullValueHandling = NullValueHandling.Ignore)] public string SecretComment { get; set; }
        [JsonProperty("skipMultilineEncoding", NullValueHandling = NullValueHandling.Ignore)] public bool? SkipMultilineEncoding { get; set; }
        [JsonProperty("tagIds", NullValueHandling = NullValueHandling.Ignore)] public string[] TagIds { get; set; }
        [JsonProperty("secretMetadata", NullValueHandling = NullValueHandling.Ignore)] public List<InfisicalSecretMetadataDto> SecretMetadata { get; set; }
    }

    internal sealed class InfisicalSecretBatchDeleteItemDto
    {
        [JsonProperty("secretKey")] public string SecretKey { get; set; }
    }

    internal sealed class InfisicalSecretBatchCreateRequestDto
    {
        [JsonProperty("workspaceId")] public string WorkspaceId { get; set; }
        [JsonProperty("environment")] public string Environment { get; set; }
        [JsonProperty("secretPath", NullValueHandling = NullValueHandling.Ignore)] public string SecretPath { get; set; }
        [JsonProperty("secrets")] public List<InfisicalSecretBatchCreateItemDto> Secrets { get; set; }
    }

    internal sealed class InfisicalSecretBatchUpdateRequestDto
    {
        [JsonProperty("workspaceId")] public string WorkspaceId { get; set; }
        [JsonProperty("environment")] public string Environment { get; set; }
        [JsonProperty("secretPath", NullValueHandling = NullValueHandling.Ignore)] public string SecretPath { get; set; }
        [JsonProperty("mode", NullValueHandling = NullValueHandling.Ignore)] public string Mode { get; set; }
        [JsonProperty("secrets")] public List<InfisicalSecretBatchUpdateItemDto> Secrets { get; set; }
    }

    internal sealed class InfisicalSecretBatchDeleteRequestDto
    {
        [JsonProperty("workspaceId")] public string WorkspaceId { get; set; }
        [JsonProperty("environment")] public string Environment { get; set; }
        [JsonProperty("secretPath", NullValueHandling = NullValueHandling.Ignore)] public string SecretPath { get; set; }
        [JsonProperty("secrets")] public List<InfisicalSecretBatchDeleteItemDto> Secrets { get; set; }
    }

    internal sealed class InfisicalSecretDuplicateAttributesDto
    {
        [JsonProperty("secretValue", NullValueHandling = NullValueHandling.Ignore)] public bool? SecretValue { get; set; }
        [JsonProperty("secretComment", NullValueHandling = NullValueHandling.Ignore)] public bool? SecretComment { get; set; }
        [JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore)] public bool? Tags { get; set; }
        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)] public bool? Metadata { get; set; }
    }

    internal sealed class InfisicalSecretDuplicateRequestDto
    {
        [JsonProperty("projectId")] public string ProjectId { get; set; }
        [JsonProperty("sourceEnvironment")] public string SourceEnvironment { get; set; }
        [JsonProperty("destinationEnvironment")] public string DestinationEnvironment { get; set; }
        [JsonProperty("sourceSecretPath", NullValueHandling = NullValueHandling.Ignore)] public string SourceSecretPath { get; set; }
        [JsonProperty("destinationSecretPath", NullValueHandling = NullValueHandling.Ignore)] public string DestinationSecretPath { get; set; }
        [JsonProperty("secretIds")] public string[] SecretIds { get; set; }
        [JsonProperty("overwriteExisting", NullValueHandling = NullValueHandling.Ignore)] public bool? OverwriteExisting { get; set; }
        [JsonProperty("attributesToCopy", NullValueHandling = NullValueHandling.Ignore)] public InfisicalSecretDuplicateAttributesDto AttributesToCopy { get; set; }
    }
}
