using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Secrets;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "InfisicalSecrets")]
    [OutputType(typeof(InfisicalSecret))]
    public sealed class GetInfisicalSecretsCmdlet : InfisicalCmdletBase
    {
        [Parameter] public string ProjectId { get; set; }
        [Parameter] public string Environment { get; set; }
        [Parameter] public string SecretPath { get; set; }
        [Parameter] public string ApiVersion { get; set; }
        [Parameter] public SwitchParameter Recursive { get; set; }
        [Parameter] public SwitchParameter IncludeImports { get; set; }
        [Parameter] public SwitchParameter IncludePersonalOverrides { get; set; }
        [Parameter] public SwitchParameter ExpandSecretReferences { get; set; }
        [Parameter] public SwitchParameter ViewSecretValue { get; set; } = SwitchParameter.Present;
        [Parameter] public Hashtable MetadataFilter { get; set; }
        [Parameter] public string[] TagSlugs { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();

                InfisicalListSecretsQuery query = new InfisicalListSecretsQuery
                {
                    ProjectId = ResolveProjectId(connection, ProjectId),
                    Environment = ResolveEnvironment(connection, Environment),
                    SecretPath = ResolveSecretPath(connection, SecretPath),
                    ApiVersion = ResolveApiVersion(connection, ApiVersion),
                    Recursive = Recursive.IsPresent,
                    IncludeImports = IncludeImports.IsPresent,
                    IncludePersonalOverrides = IncludePersonalOverrides.IsPresent,
                    ExpandSecretReferences = ExpandSecretReferences.IsPresent,
                    ViewSecretValue = ViewSecretValue.IsPresent,
                    MetadataFilter = ToStringDictionary(MetadataFilter),
                    TagSlugs = TagSlugs
                };

                InfisicalSecretsClient client = new InfisicalSecretsClient(HttpClient, Logger);
                InfisicalSecret[] secrets = client.List(connection, query);

                foreach (InfisicalSecret secret in secrets)
                {
                    WriteObject(secret);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("GetInfisicalSecretsCmdlet", "RetrieveSecrets", exception);
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
