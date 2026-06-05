using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Projects;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "InfisicalProject", DefaultParameterSetName = "List")]
    [OutputType(typeof(InfisicalProject))]
    public sealed class GetInfisicalProjectCmdlet : InfisicalCmdletBase
    {
        [Parameter(ParameterSetName = "Single", Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0)]
        [Alias("Id")]
        public string ProjectId { get; set; }

        [Parameter(ParameterSetName = "List")] public SwitchParameter List { get; set; }

        [Parameter(ParameterSetName = "List")]
        [ValidateSet("secret-manager", "cert-manager", "kms", "ssh", "secret-scanning", "pam", "ai")]
        public string Type { get; set; }

        [Parameter(ParameterSetName = "List")] public SwitchParameter IncludeRoles { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                InfisicalProjectClient client = new InfisicalProjectClient(HttpClient, Logger);

                if (string.Equals(ParameterSetName, "Single", StringComparison.Ordinal))
                {
                    InfisicalProject project = client.Retrieve(connection, ProjectId);
                    if (project != null)
                    {
                        WriteObject(project);
                    }

                    return;
                }

                InfisicalProject[] projects = client.List(connection, Type, IncludeRoles.IsPresent);
                foreach (InfisicalProject project in projects)
                {
                    WriteObject(project);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("GetInfisicalProjectCmdlet", "GetProject", exception);
            }
        }
    }
}
