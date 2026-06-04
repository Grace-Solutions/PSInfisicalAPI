using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Projects;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.Remove, "InfisicalProject", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveInfisicalProjectCmdlet : InfisicalCmdletBase
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0)]
        [Alias("Id")]
        public string ProjectId { get; set; }

        [Parameter] public SwitchParameter PassThru { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                string resolvedProjectId = ResolveProjectId(connection, ProjectId);

                if (!ShouldProcess(resolvedProjectId, "Remove Infisical project"))
                {
                    return;
                }

                InfisicalProjectClient client = new InfisicalProjectClient(HttpClient, Logger);
                client.Delete(connection, resolvedProjectId);

                if (PassThru.IsPresent)
                {
                    WriteObject(resolvedProjectId);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("RemoveInfisicalProjectCmdlet", "DeleteProject", exception);
            }
        }
    }
}
