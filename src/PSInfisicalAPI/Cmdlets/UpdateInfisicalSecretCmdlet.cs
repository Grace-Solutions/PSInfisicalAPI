using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Security;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Secrets;
using PSInfisicalAPI.Security;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsData.Update, "InfisicalSecret", SupportsShouldProcess = true, DefaultParameterSetName = "PlainText")]
    [OutputType(typeof(InfisicalSecret))]
    public sealed class UpdateInfisicalSecretCmdlet : InfisicalCmdletBase
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0, ParameterSetName = "PlainText")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0, ParameterSetName = "SecureString")]
        public string SecretName { get; set; }

        [Parameter(ParameterSetName = "PlainText")] public string SecretValue { get; set; }
        [Parameter(ParameterSetName = "SecureString")] public SecureString SecureSecretValue { get; set; }

        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Bulk", ValueFromPipeline = true)]
        [BulkSecretsTransformation]
        public IDictionary<string, string>[] Secrets { get; set; }

        [Parameter] public string NewSecretName { get; set; }
        [Parameter] public string SecretComment { get; set; }
        [Parameter] public string ProjectId { get; set; }
        [Parameter] public string Environment { get; set; }
        [Parameter] public string SecretPath { get; set; }
        [Parameter] public string ApiVersion { get; set; }
        [Parameter] public InfisicalSecretType Type { get; set; } = InfisicalSecretType.Shared;
        [Parameter] public SwitchParameter SkipMultilineEncoding { get; set; }
        [Parameter] public string[] TagIds { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                string resolvedProjectId = ResolveProjectId(connection, ProjectId);
                string resolvedEnvironment = ResolveEnvironment(connection, Environment);
                string resolvedSecretPath = ResolveSecretPath(connection, SecretPath);
                string resolvedApiVersion = ResolveApiVersion(connection, ApiVersion);

                if (string.Equals(ParameterSetName, "Bulk", StringComparison.Ordinal))
                {
                    if (Secrets == null || Secrets.Length == 0) { return; }
                    string target = string.Concat(Secrets.Length, " secret(s)");
                    if (!ShouldProcess(target, "Bulk-update Infisical secrets")) { return; }

                    InfisicalBulkUpdateSecretsRequest bulk = new InfisicalBulkUpdateSecretsRequest
                    {
                        ProjectId = resolvedProjectId,
                        Environment = resolvedEnvironment,
                        SecretPath = resolvedSecretPath,
                        ApiVersion = resolvedApiVersion,
                        Secrets = InfisicalBulkSecretConverter.ToUpdateItems(Secrets)
                    };

                    InfisicalSecretsClient bulkClient = new InfisicalSecretsClient(HttpClient, Logger);
                    InfisicalSecret[] updated = bulkClient.UpdateBatch(connection, bulk);
                    if (updated != null)
                    {
                        foreach (InfisicalSecret secret in updated) { WriteObject(secret); }
                    }

                    return;
                }

                if (!ShouldProcess(SecretName, "Update Infisical secret")) { return; }

                string plainValue = SecureSecretValue != null
                    ? SecureStringUtility.UsePlainText(SecureSecretValue, p => p)
                    : SecretValue;

                InfisicalUpdateSecretRequest request = new InfisicalUpdateSecretRequest
                {
                    SecretName = SecretName,
                    NewSecretName = NewSecretName,
                    SecretValue = plainValue,
                    SecretComment = SecretComment,
                    ProjectId = resolvedProjectId,
                    Environment = resolvedEnvironment,
                    SecretPath = resolvedSecretPath,
                    Type = Type.ToString(),
                    ApiVersion = resolvedApiVersion,
                    SkipMultilineEncoding = SkipMultilineEncoding.IsPresent ? (bool?)true : null,
                    TagIds = TagIds
                };

                InfisicalSecretsClient client = new InfisicalSecretsClient(HttpClient, Logger);
                InfisicalSecret single = client.Update(connection, request);
                if (single != null)
                {
                    WriteObject(single);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("UpdateInfisicalSecretCmdlet", "UpdateSecret", exception);
            }
        }
    }
}
