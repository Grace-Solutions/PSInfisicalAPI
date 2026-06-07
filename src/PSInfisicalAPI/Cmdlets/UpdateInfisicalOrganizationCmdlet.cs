using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Organizations;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsData.Update, "InfisicalOrganization", SupportsShouldProcess = true)]
    [OutputType(typeof(InfisicalOrganization))]
    public sealed class UpdateInfisicalOrganizationCmdlet : InfisicalCmdletBase
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0)]
        [Alias("Id")]
        public string OrganizationId { get; set; }

        [Parameter] public string Name { get; set; }
        [Parameter] public string Slug { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();

                if (!ShouldProcess(OrganizationId, "Update Infisical organization"))
                {
                    return;
                }

                InfisicalOrganizationClient client = new InfisicalOrganizationClient(HttpClient, Logger);
                InfisicalOrganization organization = client.Update(connection, OrganizationId, Name, Slug);
                if (organization != null)
                {
                    WriteObject(organization);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("UpdateInfisicalOrganizationCmdlet", "UpdateOrganization", exception);
            }
        }
    }
}
