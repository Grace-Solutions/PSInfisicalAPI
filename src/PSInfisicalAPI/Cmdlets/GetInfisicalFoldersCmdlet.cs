using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Folders;
using PSInfisicalAPI.Models;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "InfisicalFolders")]
    [OutputType(typeof(InfisicalFolder))]
    public sealed class GetInfisicalFoldersCmdlet : InfisicalCmdletBase
    {
        [Parameter] public string ProjectId { get; set; }
        [Parameter] public string Environment { get; set; }
        [Parameter] public string Path { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                string resolvedProjectId = ResolveProjectId(connection, ProjectId);
                string resolvedEnvironment = ResolveEnvironment(connection, Environment);
                string resolvedPath = ResolveSecretPath(connection, Path);
                InfisicalFolderClient client = new InfisicalFolderClient(HttpClient, Logger);
                InfisicalFolder[] folders = client.List(connection, resolvedProjectId, resolvedEnvironment, resolvedPath);
                foreach (InfisicalFolder folder in folders)
                {
                    WriteObject(folder);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("GetInfisicalFoldersCmdlet", "ListFolders", exception);
            }
        }
    }
}
