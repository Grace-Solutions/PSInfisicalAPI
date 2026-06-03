using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Tags;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.Remove, "InfisicalTag", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveInfisicalTagCmdlet : InfisicalCmdletBase
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0)]
        [Alias("Id")]
        public string TagId { get; set; }

        [Parameter] public string ProjectId { get; set; }
        [Parameter] public SwitchParameter PassThru { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                if (!ShouldProcess(TagId, "Remove Infisical tag"))
                {
                    return;
                }

                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                string resolvedProjectId = ResolveProjectId(connection, ProjectId);
                InfisicalTagClient client = new InfisicalTagClient(HttpClient, Logger);
                client.Delete(connection, resolvedProjectId, TagId);

                if (PassThru.IsPresent)
                {
                    WriteObject(TagId);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("RemoveInfisicalTagCmdlet", "DeleteTag", exception);
            }
        }
    }
}
