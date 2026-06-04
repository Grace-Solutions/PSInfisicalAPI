using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Pki;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "InfisicalCertificates")]
    [OutputType(typeof(InfisicalCertificate))]
    public sealed class GetInfisicalCertificatesCmdlet : InfisicalCmdletBase
    {
        [Parameter] public string ProjectId { get; set; }
        [Parameter] public string CommonName { get; set; }
        [Parameter] public string FriendlyName { get; set; }
        [Parameter] public string Status { get; set; }
        [Parameter] public string[] CaId { get; set; }
        [Parameter] public int? Limit { get; set; }
        [Parameter] public int? Offset { get; set; }
        [Parameter] public SwitchParameter NoAutoPage { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                InfisicalPkiClient client = new InfisicalPkiClient(HttpClient, Logger);
                string resolvedProjectId = ResolveProjectId(connection, ProjectId);

                InfisicalCertificateSearchQuery query = new InfisicalCertificateSearchQuery
                {
                    ProjectId = resolvedProjectId,
                    CommonName = CommonName,
                    FriendlyName = FriendlyName,
                    Status = Status,
                    CaIds = CaId,
                    Limit = Limit ?? 100,
                    Offset = Offset ?? 0
                };

                int requestedLimit = query.Limit ?? 100;
                int emitted = 0;
                while (true)
                {
                    InfisicalCertificateSearchResult page = client.SearchCertificates(connection, query);
                    if (page == null || page.Certificates == null || page.Certificates.Length == 0)
                    {
                        break;
                    }

                    foreach (InfisicalCertificate cert in page.Certificates)
                    {
                        WriteObject(cert);
                        emitted++;
                    }

                    if (NoAutoPage.IsPresent || page.Certificates.Length < requestedLimit)
                    {
                        break;
                    }

                    if (page.TotalCount > 0 && emitted >= page.TotalCount)
                    {
                        break;
                    }

                    query.Offset = (query.Offset ?? 0) + page.Certificates.Length;
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException("GetInfisicalCertificatesCmdlet", "GetCertificates", exception);
            }
        }
    }
}
