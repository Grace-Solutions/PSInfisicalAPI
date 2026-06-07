using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Organizations;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "InfisicalOrganization", DefaultParameterSetName = "List")]
    [OutputType(typeof(InfisicalOrganization))]
    public sealed class GetInfisicalOrganizationCmdlet : InfisicalCmdletBase
    {
        [Parameter(ParameterSetName = "Single", Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0)]
        [Alias("Id")]
        public string OrganizationId { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                InfisicalOrganizationClient client = new InfisicalOrganizationClient(HttpClient, Logger);

                if (string.Equals(ParameterSetName, "Single", StringComparison.Ordinal))
                {
                    InfisicalOrganization organization = client.Retrieve(connection, OrganizationId);
                    if (organization != null)
                    {
                        WriteObject(organization);
                    }

                    return;
                }

                InfisicalOrganization[] organizations = client.List(connection);
                foreach (InfisicalOrganization organization in organizations)
                {
                    WriteObject(organization);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("GetInfisicalOrganizationCmdlet", "GetOrganization", exception);
            }
        }
    }
}
