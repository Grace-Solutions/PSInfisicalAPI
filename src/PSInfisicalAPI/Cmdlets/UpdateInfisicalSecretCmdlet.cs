using System;
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
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0)]
        public string SecretName { get; set; }

        [Parameter(ParameterSetName = "PlainText")] public string SecretValue { get; set; }
        [Parameter(ParameterSetName = "SecureString")] public SecureString SecureSecretValue { get; set; }

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
                if (!ShouldProcess(SecretName, "Update Infisical secret"))
                {
                    return;
                }

                string plainValue = SecureSecretValue != null
                    ? SecureStringUtility.UsePlainText(SecureSecretValue, p => p)
                    : SecretValue;

                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                InfisicalUpdateSecretRequest request = new InfisicalUpdateSecretRequest
                {
                    SecretName = SecretName,
                    NewSecretName = NewSecretName,
                    SecretValue = plainValue,
                    SecretComment = SecretComment,
                    ProjectId = ProjectId,
                    Environment = Environment,
                    SecretPath = SecretPath,
                    Type = Type.ToString(),
                    ApiVersion = ApiVersion,
                    SkipMultilineEncoding = SkipMultilineEncoding.IsPresent ? (bool?)true : null,
                    TagIds = TagIds
                };

                InfisicalSecretsClient client = new InfisicalSecretsClient(HttpClient, Logger);
                InfisicalSecret secret = client.Update(connection, request);
                if (secret != null)
                {
                    WriteObject(secret);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("UpdateInfisicalSecretCmdlet", "UpdateSecret", exception);
            }
        }
    }
}
