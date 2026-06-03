using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Environments;
using PSInfisicalAPI.Models;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsData.Update, "InfisicalEnvironment", SupportsShouldProcess = true)]
    [OutputType(typeof(InfisicalEnvironment))]
    public sealed class UpdateInfisicalEnvironmentCmdlet : InfisicalCmdletBase
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0)]
        [Alias("Id")]
        public string EnvironmentId { get; set; }

        [Parameter] public string ProjectId { get; set; }
        [Parameter] public string Name { get; set; }
        [Parameter] public string Slug { get; set; }
        [Parameter] public int? Position { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                if (!ShouldProcess(EnvironmentId, "Update Infisical environment"))
                {
                    return;
                }

                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                InfisicalEnvironmentClient client = new InfisicalEnvironmentClient(HttpClient, Logger);
                InfisicalEnvironment env = client.Update(connection, ProjectId, EnvironmentId, Name, Slug, Position);
                if (env != null)
                {
                    WriteObject(env);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("UpdateInfisicalEnvironmentCmdlet", "UpdateEnvironment", exception);
            }
        }
    }
}
