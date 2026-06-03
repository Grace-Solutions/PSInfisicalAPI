using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Secrets;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.Remove, "InfisicalSecret", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, DefaultParameterSetName = "Single")]
    public sealed class RemoveInfisicalSecretCmdlet : InfisicalCmdletBase
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0, ParameterSetName = "Single")]
        public string SecretName { get; set; }

        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Bulk", ValueFromPipeline = true)]
        [Alias("Names", "SecretKeys")]
        public string[] SecretNames { get; set; }

        [Parameter] public string ProjectId { get; set; }
        [Parameter] public string Environment { get; set; }
        [Parameter] public string SecretPath { get; set; }
        [Parameter] public string ApiVersion { get; set; }
        [Parameter] public InfisicalSecretType Type { get; set; } = InfisicalSecretType.Shared;
        [Parameter] public SwitchParameter PassThru { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                string resolvedProjectId = ResolveProjectId(connection, ProjectId);
                string resolvedEnvironment = ResolveEnvironment(connection, Environment);
                string resolvedSecretPath = ResolveSecretPath(connection, SecretPath);
                string resolvedApiVersion = ResolveApiVersion(connection, ApiVersion);

                InfisicalSecretsClient client = new InfisicalSecretsClient(HttpClient, Logger);

                if (string.Equals(ParameterSetName, "Bulk", StringComparison.Ordinal))
                {
                    if (SecretNames == null || SecretNames.Length == 0) { return; }
                    string target = string.Concat(SecretNames.Length, " secret(s)");
                    if (!ShouldProcess(target, "Bulk-remove Infisical secrets")) { return; }

                    InfisicalBulkDeleteSecretsRequest bulk = new InfisicalBulkDeleteSecretsRequest
                    {
                        ProjectId = resolvedProjectId,
                        Environment = resolvedEnvironment,
                        SecretPath = resolvedSecretPath,
                        ApiVersion = resolvedApiVersion,
                        SecretNames = SecretNames
                    };

                    client.DeleteBatch(connection, bulk);

                    if (PassThru.IsPresent)
                    {
                        foreach (string name in SecretNames) { WriteObject(name); }
                    }

                    return;
                }

                if (!ShouldProcess(SecretName, "Remove Infisical secret")) { return; }

                InfisicalDeleteSecretRequest request = new InfisicalDeleteSecretRequest
                {
                    SecretName = SecretName,
                    ProjectId = resolvedProjectId,
                    Environment = resolvedEnvironment,
                    SecretPath = resolvedSecretPath,
                    Type = Type.ToString(),
                    ApiVersion = resolvedApiVersion
                };

                client.Delete(connection, request);

                if (PassThru.IsPresent)
                {
                    WriteObject(SecretName);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("RemoveInfisicalSecretCmdlet", "DeleteSecret", exception);
            }
        }
    }
}
