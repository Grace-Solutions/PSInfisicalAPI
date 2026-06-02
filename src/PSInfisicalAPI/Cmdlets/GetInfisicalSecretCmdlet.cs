using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Secrets;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "InfisicalSecret")]
    [OutputType(typeof(InfisicalSecret))]
    public sealed class GetInfisicalSecretCmdlet : InfisicalCmdletBase
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0)]
        public string SecretName { get; set; }

        [Parameter] public string ProjectId { get; set; }
        [Parameter] public string Environment { get; set; }
        [Parameter] public string SecretPath { get; set; }
        [Parameter] public int? Version { get; set; }
        [Parameter] public InfisicalSecretType Type { get; set; } = InfisicalSecretType.Shared;
        [Parameter] public bool ViewSecretValue { get; set; } = true;
        [Parameter] public bool ExpandSecretReferences { get; set; } = true;
        [Parameter] public bool IncludeImports { get; set; } = true;

        protected override void ProcessRecord()
        {
            try
            {
                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();

                InfisicalRetrieveSecretQuery query = new InfisicalRetrieveSecretQuery
                {
                    SecretName = SecretName,
                    ProjectId = ProjectId,
                    Environment = Environment,
                    SecretPath = SecretPath,
                    Version = Version,
                    Type = Type.ToString(),
                    ViewSecretValue = ViewSecretValue,
                    ExpandSecretReferences = ExpandSecretReferences,
                    IncludeImports = IncludeImports
                };

                InfisicalSecretsClient client = new InfisicalSecretsClient(HttpClient, Logger);
                InfisicalSecret secret = client.Retrieve(connection, query);

                if (secret != null)
                {
                    WriteObject(secret);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("GetInfisicalSecretCmdlet", "RetrieveSecret", exception);
            }
        }
    }
}
