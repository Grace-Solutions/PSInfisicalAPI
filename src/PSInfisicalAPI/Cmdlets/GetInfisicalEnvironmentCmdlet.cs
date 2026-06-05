using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Environments;
using PSInfisicalAPI.Models;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "InfisicalEnvironment", DefaultParameterSetName = "List")]
    [OutputType(typeof(InfisicalEnvironment))]
    public sealed class GetInfisicalEnvironmentCmdlet : InfisicalCmdletBase
    {
        [Parameter(ParameterSetName = "Single", Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0)]
        [Alias("Slug", "Id", "Environment")]
        public string EnvironmentSlugOrId { get; set; }

        [Parameter(Mandatory = true)] public string ProjectId { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                InfisicalEnvironmentClient client = new InfisicalEnvironmentClient(HttpClient, Logger);

                if (string.Equals(ParameterSetName, "Single", StringComparison.Ordinal))
                {
                    InfisicalEnvironment env = client.Retrieve(connection, ProjectId, EnvironmentSlugOrId);
                    if (env != null)
                    {
                        WriteObject(env);
                    }

                    return;
                }

                InfisicalEnvironment[] envs = client.List(connection, ProjectId);
                foreach (InfisicalEnvironment env in envs)
                {
                    WriteObject(env);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("GetInfisicalEnvironmentCmdlet", "GetEnvironment", exception);
            }
        }
    }
}
