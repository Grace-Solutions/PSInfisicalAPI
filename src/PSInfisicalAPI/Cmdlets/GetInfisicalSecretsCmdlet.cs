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
        [Parameter] public SwitchParameter Recursive { get; set; }
        [Parameter] public bool IncludeImports { get; set; } = true;
        [Parameter] public SwitchParameter IncludePersonalOverrides { get; set; }
        [Parameter] public bool ExpandSecretReferences { get; set; } = true;
        [Parameter] public bool ViewSecretValue { get; set; } = true;
        [Parameter] public Hashtable MetadataFilter { get; set; }
        [Parameter] public string[] TagSlugs { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();

                InfisicalListSecretsQuery query = new InfisicalListSecretsQuery
                {
                    ProjectId = ProjectId,
                    Environment = Environment,
                    SecretPath = SecretPath,
                    Recursive = Recursive.IsPresent,
                    IncludeImports = IncludeImports,
                    IncludePersonalOverrides = IncludePersonalOverrides.IsPresent,
                    ExpandSecretReferences = ExpandSecretReferences,
                    ViewSecretValue = ViewSecretValue,
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
