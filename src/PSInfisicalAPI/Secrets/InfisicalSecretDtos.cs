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
}
