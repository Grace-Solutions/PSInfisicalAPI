using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Secrets;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "InfisicalSecret", DefaultParameterSetName = "List")]
    [OutputType(typeof(InfisicalSecret))]
    public sealed class GetInfisicalSecretCmdlet : InfisicalCmdletBase
    {
        [Parameter(ParameterSetName = "Single", Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0)]
        public string SecretName { get; set; }

        [Parameter(Mandatory = true)] public string ProjectId { get; set; }
        [Parameter(Mandatory = true)] public string Environment { get; set; }
        [Parameter] public string SecretPath { get; set; }
        [Parameter] public string ApiVersion { get; set; }
        [Parameter] public SwitchParameter ViewSecretValue { get; set; } = SwitchParameter.Present;
        [Parameter] public SwitchParameter ExpandSecretReferences { get; set; }
        [Parameter] public SwitchParameter IncludeImports { get; set; }

        [Parameter(ParameterSetName = "Single")] public int? Version { get; set; }
        [Parameter(ParameterSetName = "Single")] public InfisicalSecretType Type { get; set; } = InfisicalSecretType.Shared;

        [Parameter(ParameterSetName = "List")] public SwitchParameter Recursive { get; set; }
        [Parameter(ParameterSetName = "List")] public SwitchParameter IncludePersonalOverrides { get; set; }
        [Parameter(ParameterSetName = "List")] public Hashtable MetadataFilter { get; set; }
        [Parameter(ParameterSetName = "List")] public string[] TagSlugs { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                InfisicalSecretsClient client = new InfisicalSecretsClient(HttpClient, Logger);

                if (string.Equals(ParameterSetName, "Single", StringComparison.Ordinal))
                {
                    InfisicalRetrieveSecretQuery query = new InfisicalRetrieveSecretQuery
                    {
                        SecretName = SecretName,
                        ProjectId = ProjectId,
                        Environment = Environment,
                        SecretPath = SecretPath,
                        ApiVersion = ResolveApiVersion(connection, ApiVersion),
                        Version = Version,
                        Type = Type.ToString(),
                        ViewSecretValue = ViewSecretValue.IsPresent,
                        ExpandSecretReferences = ExpandSecretReferences.IsPresent,
                        IncludeImports = IncludeImports.IsPresent
                    };

                    InfisicalSecret secret = client.Retrieve(connection, query);
                    if (secret != null)
                    {
                        WriteObject(secret);
                    }

                    return;
                }

                InfisicalListSecretsQuery listQuery = new InfisicalListSecretsQuery
                {
                    ProjectId = ProjectId,
                    Environment = Environment,
                    SecretPath = SecretPath,
                    ApiVersion = ResolveApiVersion(connection, ApiVersion),
                    Recursive = Recursive.IsPresent,
                    IncludeImports = IncludeImports.IsPresent,
                    IncludePersonalOverrides = IncludePersonalOverrides.IsPresent,
                    ExpandSecretReferences = ExpandSecretReferences.IsPresent,
                    ViewSecretValue = ViewSecretValue.IsPresent,
                    MetadataFilter = ToStringDictionary(MetadataFilter),
                    TagSlugs = TagSlugs
                };

                InfisicalSecret[] secrets = client.List(connection, listQuery);
                foreach (InfisicalSecret secret in secrets)
                {
                    WriteObject(secret);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("GetInfisicalSecretCmdlet", "GetSecret", exception);
            }
        }

        private static Dictionary<string, string> ToStringDictionary(Hashtable hashtable)
        {
            if (hashtable == null) { return null; }

            Dictionary<string, string> result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (DictionaryEntry entry in hashtable)
            {
                if (entry.Key == null) { continue; }
                result[entry.Key.ToString()] = entry.Value != null ? entry.Value.ToString() : null;
            }

            return result;
        }
    }
}
