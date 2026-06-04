using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Environments;
using PSInfisicalAPI.Models;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.New, "InfisicalEnvironment", SupportsShouldProcess = true)]
    [OutputType(typeof(InfisicalEnvironment))]
    public sealed class NewInfisicalEnvironmentCmdlet : InfisicalCmdletBase
    {
        [Parameter(Mandatory = true, Position = 0)] public string Name { get; set; }
        [Parameter(Mandatory = true, Position = 1)] public string Slug { get; set; }
        [Parameter] public string ProjectId { get; set; }
        [Parameter] public int? Position { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                if (!ShouldProcess(Slug, "Create Infisical environment"))
                {
                    return;
                }

                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                string resolvedProjectId = ResolveProjectId(connection, ProjectId);
                InfisicalEnvironmentClient client = new InfisicalEnvironmentClient(HttpClient, Logger);
                InfisicalEnvironment env = client.Create(connection, resolvedProjectId, Name, Slug, Position);
                if (env != null)
                {
                    WriteObject(env);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("NewInfisicalEnvironmentCmdlet", "CreateEnvironment", exception);
            }
        }
    }
}
