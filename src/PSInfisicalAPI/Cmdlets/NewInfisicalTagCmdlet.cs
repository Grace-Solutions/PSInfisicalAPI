using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Tags;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.New, "InfisicalTag", SupportsShouldProcess = true)]
    [OutputType(typeof(InfisicalTag))]
    public sealed class NewInfisicalTagCmdlet : InfisicalCmdletBase
    {
        [Parameter(Mandatory = true, Position = 0)] public string Slug { get; set; }
        [Parameter] public string Name { get; set; }
        [Parameter] public string Color { get; set; }
        [Parameter] public string ProjectId { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                if (!ShouldProcess(Slug, "Create Infisical tag"))
                {
                    return;
                }

                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                string resolvedProjectId = ResolveProjectId(connection, ProjectId);
                InfisicalTagClient client = new InfisicalTagClient(HttpClient, Logger);
                InfisicalTag tag = client.Create(connection, resolvedProjectId, Slug, Name, Color);
                if (tag != null)
                {
                    WriteObject(tag);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("NewInfisicalTagCmdlet", "CreateTag", exception);
            }
        }
    }
}
