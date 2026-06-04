using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Pki;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "InfisicalCertificateAuthority", DefaultParameterSetName = "List")]
    [OutputType(typeof(InfisicalCertificateAuthority))]
    public sealed class GetInfisicalCertificateAuthorityCmdlet : InfisicalCmdletBase
    {
        [Parameter(ParameterSetName = "ById", Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        [Alias("Id")]
        public string CaId { get; set; }

        [Parameter] public string ProjectId { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                string resolvedProjectId = ResolveProjectId(connection, ProjectId);
                InfisicalPkiClient client = new InfisicalPkiClient(HttpClient, Logger);

                if (string.Equals(ParameterSetName, "ById", StringComparison.Ordinal))
                {
                    InfisicalCertificateAuthority ca = client.GetInternalCertificateAuthority(connection, CaId, resolvedProjectId);
                    if (ca != null)
                    {
                        WriteObject(ca);
                    }

                    return;
                }

                InfisicalCertificateAuthority[] all = client.ListInternalCertificateAuthorities(connection, resolvedProjectId);
                foreach (InfisicalCertificateAuthority ca in all)
                {
                    WriteObject(ca);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("GetInfisicalCertificateAuthorityCmdlet", "GetCertificateAuthority", exception);
            }
        }
    }
}
