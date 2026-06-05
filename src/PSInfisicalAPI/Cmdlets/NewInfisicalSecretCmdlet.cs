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
    [Cmdlet(VerbsCommon.New, "InfisicalSecret", SupportsShouldProcess = true, DefaultParameterSetName = "PlainText")]
    [OutputType(typeof(InfisicalSecret))]
    public sealed class NewInfisicalSecretCmdlet : InfisicalCmdletBase
    {
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "PlainText")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "SecureString")]
        public string SecretName { get; set; }

        [Parameter(Mandatory = true, Position = 1, ParameterSetName = "PlainText")]
        public string SecretValue { get; set; }

        [Parameter(Mandatory = true, Position = 1, ParameterSetName = "SecureString")]
        public SecureString SecureSecretValue { get; set; }

        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Bulk", ValueFromPipeline = true)]
        [BulkSecretsTransformation]
        public IDictionary<string, string>[] Secrets { get; set; }

        [Parameter] public string SecretComment { get; set; }
        [Parameter(Mandatory = true)] public string ProjectId { get; set; }
        [Parameter(Mandatory = true)] public string Environment { get; set; }
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
                string resolvedApiVersion = ResolveApiVersion(connection, ApiVersion);

                if (string.Equals(ParameterSetName, "Bulk", StringComparison.Ordinal))
                {
                    if (Secrets == null || Secrets.Length == 0) { return; }
                    string target = string.Concat(Secrets.Length, " secret(s)");
                    if (!ShouldProcess(target, "Bulk-create Infisical secrets")) { return; }

                    InfisicalBulkCreateSecretsRequest bulk = new InfisicalBulkCreateSecretsRequest
                    {
                        ProjectId = ProjectId,
                        Environment = Environment,
                        SecretPath = SecretPath,
                        ApiVersion = resolvedApiVersion,
                        Secrets = InfisicalBulkSecretConverter.ToCreateItems(Secrets)
                    };

                    InfisicalSecretsClient bulkClient = new InfisicalSecretsClient(HttpClient, Logger);
                    InfisicalSecret[] created = bulkClient.CreateBatch(connection, bulk);
                    if (created != null)
                    {
                        foreach (InfisicalSecret secret in created) { WriteObject(secret); }
                    }

                    return;
                }

                if (!ShouldProcess(SecretName, "Create Infisical secret")) { return; }

                string plainValue = SecureSecretValue != null
                    ? SecureStringUtility.UsePlainText(SecureSecretValue, p => p)
                    : SecretValue;

                InfisicalCreateSecretRequest request = new InfisicalCreateSecretRequest
                {
                    SecretName = SecretName,
                    SecretValue = plainValue,
                    SecretComment = SecretComment,
                    ProjectId = ProjectId,
                    Environment = Environment,
                    SecretPath = SecretPath,
                    Type = Type.ToString(),
                    ApiVersion = resolvedApiVersion,
                    SkipMultilineEncoding = SkipMultilineEncoding.IsPresent ? (bool?)true : null,
                    TagIds = TagIds
                };

                InfisicalSecretsClient client = new InfisicalSecretsClient(HttpClient, Logger);
                InfisicalSecret single = client.Create(connection, request);
                if (single != null)
                {
                    WriteObject(single);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("NewInfisicalSecretCmdlet", "CreateSecret", exception);
            }
        }
    }
}
