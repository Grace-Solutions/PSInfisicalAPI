using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Folders;
using PSInfisicalAPI.Models;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.New, "InfisicalFolder", SupportsShouldProcess = true)]
    [OutputType(typeof(InfisicalFolder))]
    public sealed class NewInfisicalFolderCmdlet : InfisicalCmdletBase
    {
        [Parameter(Mandatory = true, Position = 0)] public string Name { get; set; }
        [Parameter] public string ProjectId { get; set; }
        [Parameter] public string Environment { get; set; }
        [Parameter] public string Path { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                if (!ShouldProcess(Name, "Create Infisical folder"))
                {
                    return;
                }

                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                string resolvedProjectId = ResolveProjectId(connection, ProjectId);
                string resolvedEnvironment = ResolveEnvironment(connection, Environment);
                string resolvedPath = ResolveSecretPath(connection, Path);
                InfisicalFolderClient client = new InfisicalFolderClient(HttpClient, Logger);
                InfisicalFolder folder = client.Create(connection, resolvedProjectId, resolvedEnvironment, Name, resolvedPath);
                if (folder != null)
                {
                    WriteObject(folder);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("NewInfisicalFolderCmdlet", "CreateFolder", exception);
            }
        }
    }
}
