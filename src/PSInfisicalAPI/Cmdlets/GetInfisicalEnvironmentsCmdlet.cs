using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Environments;
using PSInfisicalAPI.Models;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "InfisicalEnvironments")]
    [OutputType(typeof(InfisicalEnvironment))]
    public sealed class GetInfisicalEnvironmentsCmdlet : InfisicalCmdletBase
    {
        [Parameter] public string ProjectId { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                string resolvedProjectId = ResolveProjectId(connection, ProjectId);
                InfisicalEnvironmentClient client = new InfisicalEnvironmentClient(HttpClient, Logger);
                InfisicalEnvironment[] envs = client.List(connection, resolvedProjectId);
                foreach (InfisicalEnvironment env in envs)
                {
                    WriteObject(env);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("GetInfisicalEnvironmentsCmdlet", "ListEnvironments", exception);
            }
        }
    }
}
