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
        [Parameter] public string ApiVersion { get; set; }
        [Parameter] public int? Version { get; set; }
        [Parameter] public InfisicalSecretType Type { get; set; } = InfisicalSecretType.Shared;
        [Parameter] public SwitchParameter ViewSecretValue { get; set; } = SwitchParameter.Present;
        [Parameter] public SwitchParameter ExpandSecretReferences { get; set; }
        [Parameter] public SwitchParameter IncludeImports { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();

                InfisicalRetrieveSecretQuery query = new InfisicalRetrieveSecretQuery
                {
                    SecretName = SecretName,
                    ProjectId = ResolveProjectId(connection, ProjectId),
                    Environment = ResolveEnvironment(connection, Environment),
                    SecretPath = ResolveSecretPath(connection, SecretPath),
                    ApiVersion = ResolveApiVersion(connection, ApiVersion),
                    Version = Version,
                    Type = Type.ToString(),
                    ViewSecretValue = ViewSecretValue.IsPresent,
                    ExpandSecretReferences = ExpandSecretReferences.IsPresent,
                    IncludeImports = IncludeImports.IsPresent
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
