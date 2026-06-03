using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Projects;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsData.Update, "InfisicalProject", SupportsShouldProcess = true)]
    [OutputType(typeof(InfisicalProject))]
    public sealed class UpdateInfisicalProjectCmdlet : InfisicalCmdletBase
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0)]
        [Alias("Id")]
        public string ProjectId { get; set; }

        [Parameter] public string Name { get; set; }
        [Parameter] public string Description { get; set; }
        [Parameter] public bool? AutoCapitalization { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                string resolvedProjectId = ResolveProjectId(connection, ProjectId);

                if (!ShouldProcess(resolvedProjectId, "Update Infisical project"))
                {
                    return;
                }

                InfisicalProjectClient client = new InfisicalProjectClient(HttpClient, Logger);
                InfisicalProject project = client.Update(connection, resolvedProjectId, Name, Description, AutoCapitalization);
                if (project != null)
                {
                    WriteObject(project);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("UpdateInfisicalProjectCmdlet", "UpdateProject", exception);
            }
        }
    }
}
