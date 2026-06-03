using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Projects;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.New, "InfisicalProject", SupportsShouldProcess = true)]
    [OutputType(typeof(InfisicalProject))]
    public sealed class NewInfisicalProjectCmdlet : InfisicalCmdletBase
    {
        [Parameter(Mandatory = true, Position = 0)]
        [Alias("Name")]
        public string ProjectName { get; set; }

        [Parameter] public string Slug { get; set; }
        [Parameter] public string Description { get; set; }
        [Parameter] public string Type { get; set; }
        [Parameter] public string OrganizationId { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                if (!ShouldProcess(ProjectName, "Create Infisical project"))
                {
                    return;
                }

                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                string resolvedOrgId = !string.IsNullOrEmpty(OrganizationId) ? OrganizationId : connection.OrganizationId;

                InfisicalProjectClient client = new InfisicalProjectClient(HttpClient, Logger);
                InfisicalProject project = client.Create(connection, ProjectName, Slug, Description, Type, resolvedOrgId);
                if (project != null)
                {
                    WriteObject(project);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("NewInfisicalProjectCmdlet", "CreateProject", exception);
            }
        }
    }
}
