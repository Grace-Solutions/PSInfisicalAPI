using System;
using System.Collections.Generic;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Pki;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.Search, "InfisicalCertificate")]
    [OutputType(typeof(InfisicalCertificate))]
    public sealed class SearchInfisicalCertificateCmdlet : InfisicalCmdletBase
    {
        [Parameter(Mandatory = true)] public string ProjectId { get; set; }
        [Parameter] public string FriendlyName { get; set; }
        [Parameter] public string CommonName { get; set; }
        [Parameter] public string Search { get; set; }
        [Parameter] public string Status { get; set; }
        [Parameter] public string[] CaId { get; set; }
        [Parameter] public string[] ProfileId { get; set; }
        [Parameter] public string[] ApplicationId { get; set; }
        [Parameter] public string[] EnrollmentType { get; set; }
        [Parameter] public string ExtendedKeyUsage { get; set; }
        [Parameter] public string[] KeyAlgorithm { get; set; }
        [Parameter] public string SignatureAlgorithm { get; set; }
        [Parameter] public string[] Source { get; set; }
        [Parameter] public DateTimeOffset? NotAfterFrom { get; set; }
        [Parameter] public DateTimeOffset? NotAfterTo { get; set; }
        [Parameter] public DateTimeOffset? NotBeforeFrom { get; set; }
        [Parameter] public DateTimeOffset? NotBeforeTo { get; set; }
        [Parameter] public string SortBy { get; set; }
        [Parameter] [ValidateSet("asc", "desc")] public string SortOrder { get; set; }
        [Parameter] public int? Limit { get; set; }
        [Parameter] public int? Offset { get; set; }
        [Parameter] public SwitchParameter NoAutoPage { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                InfisicalPkiClient client = new InfisicalPkiClient(HttpClient, Logger);

                InfisicalCertificateSearchQuery query = BuildQuery(ProjectId);
                int requestedLimit = query.Limit ?? 100;
                query.Limit = requestedLimit;
                query.Offset = query.Offset ?? 0;

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
                ThrowTerminatingForException("SearchInfisicalCertificateCmdlet", "SearchCertificates", exception);
            }
        }

        private InfisicalCertificateSearchQuery BuildQuery(string projectId)
        {
            return new InfisicalCertificateSearchQuery
            {
                ProjectId = projectId,
                FriendlyName = FriendlyName,
                CommonName = CommonName,
                Search = Search,
                Status = Status,
                CaIds = CaId,
                ProfileIds = ProfileId,
                ApplicationIds = ApplicationId,
                EnrollmentTypes = EnrollmentType,
                ExtendedKeyUsage = ExtendedKeyUsage,
                KeyAlgorithm = KeyAlgorithm,
                SignatureAlgorithm = SignatureAlgorithm,
                Source = Source,
                NotAfterFrom = NotAfterFrom,
                NotAfterTo = NotAfterTo,
                NotBeforeFrom = NotBeforeFrom,
                NotBeforeTo = NotBeforeTo,
                SortBy = SortBy,
                SortOrder = SortOrder,
                Limit = Limit,
                Offset = Offset
            };
        }
    }
}
