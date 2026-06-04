using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Pki;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "InfisicalCertificatePolicy", DefaultParameterSetName = "List")]
    [OutputType(typeof(InfisicalCertificatePolicy))]
    public sealed class GetInfisicalCertificatePolicyCmdlet : InfisicalCmdletBase
    {
        [Parameter(ParameterSetName = "ById", Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        [Alias("Id", "CertificatePolicyId")]
        public string PolicyId { get; set; }

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
                    InfisicalCertificatePolicy policy = client.GetCertificatePolicy(connection, PolicyId, resolvedProjectId);
                    if (policy != null)
                    {
                        WriteObject(policy);
                    }

                    return;
                }

                InfisicalCertificatePolicy[] all = client.ListCertificatePolicies(connection, resolvedProjectId, Limit, Offset);
                foreach (InfisicalCertificatePolicy policy in all)
                {
                    WriteObject(policy);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("GetInfisicalCertificatePolicyCmdlet", "GetCertificatePolicy", exception);
            }
        }
    }
}
