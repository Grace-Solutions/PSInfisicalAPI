using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Tags;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "InfisicalTag")]
    [OutputType(typeof(InfisicalTag))]
    public sealed class GetInfisicalTagCmdlet : InfisicalCmdletBase
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0)]
        [Alias("Slug", "Id")]
        public string TagSlugOrId { get; set; }

        [Parameter] public string ProjectId { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                string resolvedProjectId = ResolveProjectId(connection, ProjectId);
                InfisicalTagClient client = new InfisicalTagClient(HttpClient, Logger);
                InfisicalTag tag = client.Retrieve(connection, resolvedProjectId, TagSlugOrId);
                if (tag != null)
                {
                    WriteObject(tag);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("GetInfisicalTagCmdlet", "RetrieveTag", exception);
            }
        }
    }
}
