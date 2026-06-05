using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Secrets;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.Copy, "InfisicalSecret", SupportsShouldProcess = true)]
    [OutputType(typeof(InfisicalSecret))]
    public sealed class CopyInfisicalSecretCmdlet : InfisicalCmdletBase
    {
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [Alias("Id", "SecretIds")]
        public string[] SecretId { get; set; }

        [Parameter(Mandatory = true)]
        public string DestinationEnvironment { get; set; }

        [Parameter] public string DestinationSecretPath { get; set; }
        [Parameter(Mandatory = true)] public string SourceEnvironment { get; set; }
        [Parameter] public string SourceSecretPath { get; set; }
        [Parameter(Mandatory = true)] public string ProjectId { get; set; }
        [Parameter] public string ApiVersion { get; set; }
        [Parameter] public SwitchParameter OverwriteExisting { get; set; }
        [Parameter] public SwitchParameter CopySecretValue { get; set; }
        [Parameter] public SwitchParameter CopySecretComment { get; set; }
        [Parameter] public SwitchParameter CopyTags { get; set; }
        [Parameter] public SwitchParameter CopyMetadata { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                if (SecretId == null || SecretId.Length == 0) { return; }

                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                string resolvedApiVersion = ResolveApiVersion(connection, ApiVersion);

                string target = string.Concat(SecretId.Length, " secret(s) -> ", DestinationEnvironment);
                if (!ShouldProcess(target, "Duplicate Infisical secrets")) { return; }

                InfisicalDuplicateSecretsRequest request = new InfisicalDuplicateSecretsRequest
                {
                    ProjectId = ProjectId,
                    SourceEnvironment = SourceEnvironment,
                    DestinationEnvironment = DestinationEnvironment,
                    SourceSecretPath = SourceSecretPath,
                    DestinationSecretPath = DestinationSecretPath,
                    SecretIds = SecretId,
                    ApiVersion = resolvedApiVersion,
                    OverwriteExisting = OverwriteExisting.IsPresent ? (bool?)true : null,
                    CopySecretValue = CopySecretValue.IsPresent ? (bool?)true : null,
                    CopySecretComment = CopySecretComment.IsPresent ? (bool?)true : null,
                    CopyTags = CopyTags.IsPresent ? (bool?)true : null,
                    CopyMetadata = CopyMetadata.IsPresent ? (bool?)true : null
                };

                InfisicalSecretsClient client = new InfisicalSecretsClient(HttpClient, Logger);
                InfisicalSecret[] duplicated = client.Duplicate(connection, request);
                if (duplicated != null)
                {
                    foreach (InfisicalSecret secret in duplicated) { WriteObject(secret); }
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("CopyInfisicalSecretCmdlet", "DuplicateSecrets", exception);
            }
        }
    }
}
