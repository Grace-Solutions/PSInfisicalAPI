using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Environments;
using PSInfisicalAPI.Models;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "InfisicalEnvironment")]
    [OutputType(typeof(InfisicalEnvironment))]
    public sealed class GetInfisicalEnvironmentCmdlet : InfisicalCmdletBase
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0)]
        [Alias("Slug", "Id", "Environment")]
        public string EnvironmentSlugOrId { get; set; }

        [Parameter] public string ProjectId { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                string resolvedProjectId = ResolveProjectId(connection, ProjectId);
                InfisicalEnvironmentClient client = new InfisicalEnvironmentClient(HttpClient, Logger);
                InfisicalEnvironment env = client.Retrieve(connection, resolvedProjectId, EnvironmentSlugOrId);
                if (env != null)
                {
                    WriteObject(env);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("GetInfisicalEnvironmentCmdlet", "RetrieveEnvironment", exception);
            }
        }
    }
}
