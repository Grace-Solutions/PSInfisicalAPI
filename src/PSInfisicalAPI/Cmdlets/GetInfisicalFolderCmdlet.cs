using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Folders;
using PSInfisicalAPI.Models;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "InfisicalFolder", DefaultParameterSetName = "List")]
    [OutputType(typeof(InfisicalFolder))]
    public sealed class GetInfisicalFolderCmdlet : InfisicalCmdletBase
    {
        [Parameter(ParameterSetName = "Single", Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0)]
        [Alias("Name", "Id")]
        public string FolderNameOrId { get; set; }

        [Parameter(Mandatory = true)] public string ProjectId { get; set; }
        [Parameter(Mandatory = true)] public string Environment { get; set; }
        [Parameter] public string Path { get; set; }

        [Parameter(ParameterSetName = "List")] public SwitchParameter List { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                InfisicalFolderClient client = new InfisicalFolderClient(HttpClient, Logger);

                if (string.Equals(ParameterSetName, "Single", StringComparison.Ordinal))
                {
                    InfisicalFolder folder = client.Retrieve(connection, ProjectId, Environment, Path, FolderNameOrId);
                    if (folder != null)
                    {
                        WriteObject(folder);
                    }

                    return;
                }

                InfisicalFolder[] folders = client.List(connection, ProjectId, Environment, Path);
                foreach (InfisicalFolder folder in folders)
                {
                    WriteObject(folder);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("GetInfisicalFolderCmdlet", "GetFolder", exception);
            }
        }
    }
}
