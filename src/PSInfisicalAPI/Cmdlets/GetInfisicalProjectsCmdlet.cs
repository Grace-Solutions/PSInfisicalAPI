using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Projects;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "InfisicalProjects")]
    [OutputType(typeof(InfisicalProject))]
    public sealed class GetInfisicalProjectsCmdlet : InfisicalCmdletBase
    {
        protected override void ProcessRecord()
        {
            try
            {
                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                InfisicalProjectClient client = new InfisicalProjectClient(HttpClient, Logger);
                InfisicalProject[] projects = client.List(connection);

                foreach (InfisicalProject project in projects)
                {
                    WriteObject(project);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("GetInfisicalProjectsCmdlet", "ListProjects", exception);
            }
        }
    }
}
