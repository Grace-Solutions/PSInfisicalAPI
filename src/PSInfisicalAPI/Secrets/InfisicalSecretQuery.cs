using System.Collections.Generic;

namespace PSInfisicalAPI.Secrets
{
    public sealed class InfisicalListSecretsQuery
    {
        public string ProjectId { get; set; }
        public string Environment { get; set; }
        public string SecretPath { get; set; }
        public string ApiVersion { get; set; }
        public bool Recursive { get; set; }
        public bool? IncludeImports { get; set; }
        public bool IncludePersonalOverrides { get; set; }
        public bool? ExpandSecretReferences { get; set; }
        public bool? ViewSecretValue { get; set; }
        public Dictionary<string, string> MetadataFilter { get; set; }
        public string[] TagSlugs { get; set; }
    }

    public sealed class InfisicalRetrieveSecretQuery
    {
        public string SecretName { get; set; }
        public string ProjectId { get; set; }
        public string Environment { get; set; }
        public string SecretPath { get; set; }
        public string ApiVersion { get; set; }
        public int? Version { get; set; }
        public string Type { get; set; }
        public bool? ViewSecretValue { get; set; }
        public bool? ExpandSecretReferences { get; set; }
        public bool? IncludeImports { get; set; }
    }

    public sealed class InfisicalCreateSecretRequest
    {
        public string SecretName { get; set; }
        public string SecretValue { get; set; }
        public string SecretComment { get; set; }
        public string ProjectId { get; set; }
        public string Environment { get; set; }
        public string SecretPath { get; set; }
        public string Type { get; set; }
        public string ApiVersion { get; set; }
        public bool? SkipMultilineEncoding { get; set; }
        public string[] TagIds { get; set; }
    }

    public sealed class InfisicalUpdateSecretRequest
    {
        public string SecretName { get; set; }
        public string NewSecretName { get; set; }
        public string SecretValue { get; set; }
        public string SecretComment { get; set; }
        public string ProjectId { get; set; }
        public string Environment { get; set; }
        public string SecretPath { get; set; }
        public string Type { get; set; }
        public string ApiVersion { get; set; }
        public bool? SkipMultilineEncoding { get; set; }
        public string[] TagIds { get; set; }
    }

    public sealed class InfisicalDeleteSecretRequest
    {
        public string SecretName { get; set; }
        public string ProjectId { get; set; }
        public string Environment { get; set; }
        public string SecretPath { get; set; }
        public string Type { get; set; }
        public string ApiVersion { get; set; }
    }

    public sealed class InfisicalBulkCreateSecretItem
    {
        public string SecretName { get; set; }
        public string SecretValue { get; set; }
        public string SecretComment { get; set; }
        public bool? SkipMultilineEncoding { get; set; }
        public string[] TagIds { get; set; }
        public Dictionary<string, string> SecretMetadata { get; set; }
    }

    public sealed class InfisicalBulkUpdateSecretItem
    {
        public string SecretName { get; set; }
        public string NewSecretName { get; set; }
        public string SecretValue { get; set; }
        public string SecretComment { get; set; }
        public bool? SkipMultilineEncoding { get; set; }
        public string[] TagIds { get; set; }
        public Dictionary<string, string> SecretMetadata { get; set; }
    }

    public sealed class InfisicalBulkCreateSecretsRequest
    {
        public string ProjectId { get; set; }
        public string Environment { get; set; }
        public string SecretPath { get; set; }
        public string ApiVersion { get; set; }
        public InfisicalBulkCreateSecretItem[] Secrets { get; set; }
    }

    public sealed class InfisicalBulkUpdateSecretsRequest
    {
        public string ProjectId { get; set; }
        public string Environment { get; set; }
        public string SecretPath { get; set; }
        public string ApiVersion { get; set; }
        public string Mode { get; set; }
        public InfisicalBulkUpdateSecretItem[] Secrets { get; set; }
    }

    public sealed class InfisicalBulkDeleteSecretsRequest
    {
        public string ProjectId { get; set; }
        public string Environment { get; set; }
        public string SecretPath { get; set; }
        public string ApiVersion { get; set; }
        public string[] SecretNames { get; set; }
    }

    public sealed class InfisicalDuplicateSecretsRequest
    {
        public string ProjectId { get; set; }
        public string SourceEnvironment { get; set; }
        public string DestinationEnvironment { get; set; }
        public string SourceSecretPath { get; set; }
        public string DestinationSecretPath { get; set; }
        public string[] SecretIds { get; set; }
        public bool? OverwriteExisting { get; set; }
        public bool? CopySecretValue { get; set; }
        public bool? CopySecretComment { get; set; }
        public bool? CopyTags { get; set; }
        public bool? CopyMetadata { get; set; }
        public string ApiVersion { get; set; }
    }
}
