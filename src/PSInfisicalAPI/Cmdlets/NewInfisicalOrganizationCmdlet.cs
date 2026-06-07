using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Organizations;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.New, "InfisicalOrganization", SupportsShouldProcess = true)]
    [OutputType(typeof(InfisicalOrganization))]
    public sealed class NewInfisicalOrganizationCmdlet : InfisicalCmdletBase
    {
        [Parameter(Mandatory = true, Position = 0)] public string Name { get; set; }
        [Parameter] public string Slug { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                if (!ShouldProcess(Name, "Create Infisical organization"))
                {
                    return;
                }

                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                InfisicalOrganizationClient client = new InfisicalOrganizationClient(HttpClient, Logger);
                InfisicalOrganization organization = client.Create(connection, Name, Slug);
                if (organization != null)
                {
                    WriteObject(organization);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("NewInfisicalOrganizationCmdlet", "CreateOrganization", exception);
            }
        }
    }
}
