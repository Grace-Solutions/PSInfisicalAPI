using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.SubOrganizations;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.Remove, "InfisicalSubOrganization", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveInfisicalSubOrganizationCmdlet : InfisicalCmdletBase
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0)]
        [Alias("Id")]
        public string SubOrganizationId { get; set; }

        [Parameter] public SwitchParameter PassThru { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();

                if (!ShouldProcess(SubOrganizationId, "Remove Infisical sub-organization"))
                {
                    return;
                }

                InfisicalSubOrganizationClient client = new InfisicalSubOrganizationClient(HttpClient, Logger);
                client.Delete(connection, SubOrganizationId);

                if (PassThru.IsPresent)
                {
                    WriteObject(SubOrganizationId);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("RemoveInfisicalSubOrganizationCmdlet", "DeleteSubOrganization", exception);
            }
        }
    }
}
