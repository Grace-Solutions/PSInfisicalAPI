using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.SubOrganizations;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsData.Update, "InfisicalSubOrganization", SupportsShouldProcess = true)]
    [OutputType(typeof(InfisicalSubOrganization))]
    public sealed class UpdateInfisicalSubOrganizationCmdlet : InfisicalCmdletBase
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0)]
        [Alias("Id")]
        public string SubOrganizationId { get; set; }

        [Parameter] public string Name { get; set; }
        [Parameter] public string Slug { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();

                if (!ShouldProcess(SubOrganizationId, "Update Infisical sub-organization"))
                {
                    return;
                }

                InfisicalSubOrganizationClient client = new InfisicalSubOrganizationClient(HttpClient, Logger);
                InfisicalSubOrganization subOrganization = client.Update(connection, SubOrganizationId, Name, Slug);
                if (subOrganization != null)
                {
                    WriteObject(subOrganization);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("UpdateInfisicalSubOrganizationCmdlet", "UpdateSubOrganization", exception);
            }
        }
    }
}
