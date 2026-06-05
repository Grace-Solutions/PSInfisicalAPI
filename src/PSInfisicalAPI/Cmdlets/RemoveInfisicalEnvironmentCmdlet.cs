using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Environments;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.Remove, "InfisicalEnvironment", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveInfisicalEnvironmentCmdlet : InfisicalCmdletBase
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0)]
        [Alias("Id")]
        public string EnvironmentId { get; set; }

        [Parameter(Mandatory = true)] public string ProjectId { get; set; }
        [Parameter] public SwitchParameter PassThru { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                if (!ShouldProcess(EnvironmentId, "Remove Infisical environment"))
                {
                    return;
                }

                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                InfisicalEnvironmentClient client = new InfisicalEnvironmentClient(HttpClient, Logger);
                client.Delete(connection, ProjectId, EnvironmentId);

                if (PassThru.IsPresent)
                {
                    WriteObject(EnvironmentId);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("RemoveInfisicalEnvironmentCmdlet", "DeleteEnvironment", exception);
            }
        }
    }
}
