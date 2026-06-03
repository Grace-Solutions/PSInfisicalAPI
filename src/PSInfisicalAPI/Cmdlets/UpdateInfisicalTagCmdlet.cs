using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Tags;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsData.Update, "InfisicalTag", SupportsShouldProcess = true)]
    [OutputType(typeof(InfisicalTag))]
    public sealed class UpdateInfisicalTagCmdlet : InfisicalCmdletBase
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0)]
        [Alias("Id")]
        public string TagId { get; set; }

        [Parameter] public string Slug { get; set; }
        [Parameter] public string Name { get; set; }
        [Parameter] public string Color { get; set; }
        [Parameter] public string ProjectId { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                if (!ShouldProcess(TagId, "Update Infisical tag"))
                {
                    return;
                }

                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                string resolvedProjectId = ResolveProjectId(connection, ProjectId);
                InfisicalTagClient client = new InfisicalTagClient(HttpClient, Logger);
                InfisicalTag tag = client.Update(connection, resolvedProjectId, TagId, Slug, Name, Color);
                if (tag != null)
                {
                    WriteObject(tag);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("UpdateInfisicalTagCmdlet", "UpdateTag", exception);
            }
        }
    }
}
