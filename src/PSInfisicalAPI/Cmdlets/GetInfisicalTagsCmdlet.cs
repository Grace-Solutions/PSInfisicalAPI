using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Tags;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "InfisicalTags")]
    [OutputType(typeof(InfisicalTag))]
    public sealed class GetInfisicalTagsCmdlet : InfisicalCmdletBase
    {
        [Parameter] public string ProjectId { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                InfisicalTagClient client = new InfisicalTagClient(HttpClient, Logger);
                InfisicalTag[] tags = client.List(connection, ProjectId);
                foreach (InfisicalTag tag in tags)
                {
                    WriteObject(tag);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("GetInfisicalTagsCmdlet", "ListTags", exception);
            }
        }
    }
}
