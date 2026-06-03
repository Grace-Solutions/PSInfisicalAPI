using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Secrets;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.Remove, "InfisicalSecret", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveInfisicalSecretCmdlet : InfisicalCmdletBase
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0)]
        public string SecretName { get; set; }

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
                if (!ShouldProcess(SecretName, "Remove Infisical secret"))
                {
                    return;
                }

                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                InfisicalDeleteSecretRequest request = new InfisicalDeleteSecretRequest
                {
                    SecretName = SecretName,
                    ProjectId = ProjectId,
                    Environment = Environment,
                    SecretPath = SecretPath,
                    Type = Type.ToString(),
                    ApiVersion = ApiVersion
                };

                InfisicalSecretsClient client = new InfisicalSecretsClient(HttpClient, Logger);
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
