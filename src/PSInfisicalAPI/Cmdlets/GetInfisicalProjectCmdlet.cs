using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Projects;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "InfisicalProject")]
    [OutputType(typeof(InfisicalProject))]
    public sealed class GetInfisicalProjectCmdlet : InfisicalCmdletBase
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0)]
        [Alias("Id")]
        public string ProjectId { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                string resolvedProjectId = ResolveProjectId(connection, ProjectId);
                InfisicalProjectClient client = new InfisicalProjectClient(HttpClient, Logger);
                InfisicalProject project = client.Retrieve(connection, resolvedProjectId);
                if (project != null)
                {
                    WriteObject(project);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("GetInfisicalProjectCmdlet", "RetrieveProject", exception);
            }
        }
    }
}
