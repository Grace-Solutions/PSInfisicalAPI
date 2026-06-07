using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.SubOrganizations;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.New, "InfisicalSubOrganization", SupportsShouldProcess = true)]
    [OutputType(typeof(InfisicalSubOrganization))]
    public sealed class NewInfisicalSubOrganizationCmdlet : InfisicalCmdletBase
    {
        [Parameter(Mandatory = true, Position = 0)] public string Name { get; set; }
        [Parameter(Mandatory = true, Position = 1)] public string Slug { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                if (!ShouldProcess(Name, "Create Infisical sub-organization"))
                {
                    return;
                }

                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                InfisicalSubOrganizationClient client = new InfisicalSubOrganizationClient(HttpClient, Logger);
                InfisicalSubOrganization subOrganization = client.Create(connection, Name, Slug);
                if (subOrganization != null)
                {
                    WriteObject(subOrganization);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("NewInfisicalSubOrganizationCmdlet", "CreateSubOrganization", exception);
            }
        }
    }
}
