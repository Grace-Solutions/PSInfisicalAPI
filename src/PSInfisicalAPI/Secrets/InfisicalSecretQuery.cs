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
}
