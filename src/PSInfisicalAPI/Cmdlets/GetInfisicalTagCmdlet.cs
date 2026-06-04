using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Tags;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "InfisicalTag", DefaultParameterSetName = "List")]
    [OutputType(typeof(InfisicalTag))]
    public sealed class GetInfisicalTagCmdlet : InfisicalCmdletBase
    {
        [Parameter(ParameterSetName = "Single", Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0)]
        [Alias("Slug", "Id")]
        public string TagSlugOrId { get; set; }

        [Parameter] public string ProjectId { get; set; }

        [Parameter(ParameterSetName = "List")] public SwitchParameter List { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                string resolvedProjectId = ResolveProjectId(connection, ProjectId);
                InfisicalTagClient client = new InfisicalTagClient(HttpClient, Logger);

                if (string.Equals(ParameterSetName, "Single", StringComparison.Ordinal))
                {
                    InfisicalTag tag = client.Retrieve(connection, resolvedProjectId, TagSlugOrId);
                    if (tag != null)
                    {
                        WriteObject(tag);
                    }

                    return;
                }

                InfisicalTag[] tags = client.List(connection, resolvedProjectId);
                foreach (InfisicalTag tag in tags)
                {
                    WriteObject(tag);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("GetInfisicalTagCmdlet", "GetTag", exception);
            }
        }
    }
}
