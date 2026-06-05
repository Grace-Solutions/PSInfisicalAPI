using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Pki;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "InfisicalCertificateApplication", DefaultParameterSetName = "List")]
    [OutputType(typeof(InfisicalCertificateApplication))]
    public sealed class GetInfisicalCertificateApplicationCmdlet : InfisicalCmdletBase
    {
        [Parameter(ParameterSetName = "ById", Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        [Alias("ApplicationId")]
        public string Id { get; set; }

        [Parameter(ParameterSetName = "ByName", Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        [Alias("Name")]
        public string ApplicationName { get; set; }

        [Parameter] public string ProjectId { get; set; }

        [Parameter(ParameterSetName = "List")] public int? Limit { get; set; }

        [Parameter(ParameterSetName = "List")] public int? Offset { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                InfisicalPkiClient client = new InfisicalPkiClient(HttpClient, Logger);
                string resolvedProjectId = ResolveProjectId(connection, ProjectId);

                if (string.Equals(ParameterSetName, "ById", StringComparison.Ordinal))
                {
                    InfisicalCertificateApplication app = client.GetCertificateApplication(connection, Id, resolvedProjectId);
                    if (app != null) { WriteObject(app); }
                    return;
                }

                if (string.Equals(ParameterSetName, "ByName", StringComparison.Ordinal))
                {
                    InfisicalCertificateApplication app = client.GetCertificateApplicationByName(connection, ApplicationName, resolvedProjectId);
                    if (app != null) { WriteObject(app); }
                    return;
                }

                InfisicalCertificateApplication[] all = client.ListCertificateApplications(connection, resolvedProjectId, Limit, Offset);
                foreach (InfisicalCertificateApplication app in all)
                {
                    WriteObject(app);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("GetInfisicalCertificateApplicationCmdlet", "GetCertificateApplication", exception);
            }
        }
    }
}
