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

        [Parameter(Mandatory = true)] public string ProjectId { get; set; }

        [Parameter(ParameterSetName = "List")]
        [ValidateSet("Internal", "Acme", "Any")]
        public string Kind { get; set; } = "Internal";

        protected override void ProcessRecord()
        {
            try
            {
                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                InfisicalPkiClient client = new InfisicalPkiClient(HttpClient, Logger);

                if (string.Equals(ParameterSetName, "ById", StringComparison.Ordinal))
                {
                    InfisicalCertificateAuthority ca = client.GetInternalCertificateAuthority(connection, CaId, ProjectId);
                    if (ca != null)
                    {
                        WriteObject(ca);
                    }

                    return;
                }

                InfisicalCertificateAuthority[] all;
                if (string.Equals(Kind, "Internal", StringComparison.OrdinalIgnoreCase))
                {
                    all = client.ListInternalCertificateAuthorities(connection, ProjectId);
                }
                else
                {
                    all = client.ListAllCertificateAuthorities(connection, ProjectId);
                    if (string.Equals(Kind, "Acme", StringComparison.OrdinalIgnoreCase))
                    {
                        all = FilterByType(all, "acme");
                    }
                }

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

        private static InfisicalCertificateAuthority[] FilterByType(InfisicalCertificateAuthority[] source, string type)
        {
            if (source == null || source.Length == 0) { return Array.Empty<InfisicalCertificateAuthority>(); }
            System.Collections.Generic.List<InfisicalCertificateAuthority> kept = new System.Collections.Generic.List<InfisicalCertificateAuthority>();
            foreach (InfisicalCertificateAuthority ca in source)
            {
                if (ca != null && string.Equals(ca.Type, type, StringComparison.OrdinalIgnoreCase))
                {
                    kept.Add(ca);
                }
            }

            return kept.ToArray();
        }
    }
}
