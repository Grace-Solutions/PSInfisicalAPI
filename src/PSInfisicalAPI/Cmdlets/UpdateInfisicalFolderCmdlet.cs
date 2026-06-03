using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Folders;
using PSInfisicalAPI.Models;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsData.Update, "InfisicalFolder", SupportsShouldProcess = true)]
    [OutputType(typeof(InfisicalFolder))]
    public sealed class UpdateInfisicalFolderCmdlet : InfisicalCmdletBase
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0)]
        [Alias("Id")]
        public string FolderId { get; set; }

        [Parameter(Mandatory = true, Position = 1)] public string Name { get; set; }
        [Parameter] public string ProjectId { get; set; }
        [Parameter] public string Environment { get; set; }
        [Parameter] public string Path { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                if (!ShouldProcess(FolderId, "Update Infisical folder"))
                {
                    return;
                }

                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                string resolvedProjectId = ResolveProjectId(connection, ProjectId);
                string resolvedEnvironment = ResolveEnvironment(connection, Environment);
                string resolvedPath = ResolveSecretPath(connection, Path);
                InfisicalFolderClient client = new InfisicalFolderClient(HttpClient, Logger);
                InfisicalFolder folder = client.Update(connection, resolvedProjectId, resolvedEnvironment, FolderId, Name, resolvedPath);
                if (folder != null)
                {
                    WriteObject(folder);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("UpdateInfisicalFolderCmdlet", "UpdateFolder", exception);
            }
        }
    }
}
