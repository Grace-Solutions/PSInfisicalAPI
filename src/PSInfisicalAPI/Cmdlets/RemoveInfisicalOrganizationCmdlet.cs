using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Organizations;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.Remove, "InfisicalOrganization", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveInfisicalOrganizationCmdlet : InfisicalCmdletBase
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0)]
        [Alias("Id")]
        public string OrganizationId { get; set; }

        [Parameter] public SwitchParameter PassThru { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();

                if (!ShouldProcess(OrganizationId, "Remove Infisical organization"))
                {
                    return;
                }

                InfisicalOrganizationClient client = new InfisicalOrganizationClient(HttpClient, Logger);
                client.Delete(connection, OrganizationId);

                if (PassThru.IsPresent)
                {
                    WriteObject(OrganizationId);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("RemoveInfisicalOrganizationCmdlet", "DeleteOrganization", exception);
            }
        }
    }
}
