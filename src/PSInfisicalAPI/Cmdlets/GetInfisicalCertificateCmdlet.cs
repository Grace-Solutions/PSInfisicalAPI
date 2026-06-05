using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Pki;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "InfisicalCertificate", DefaultParameterSetName = "List")]
    [OutputType(typeof(InfisicalCertificate))]
    public sealed class GetInfisicalCertificateCmdlet : InfisicalCmdletBase
    {
        [Parameter(ParameterSetName = "Single", Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        [Alias("Id", "Identifier")]
        public string SerialNumber { get; set; }

        [Parameter(ParameterSetName = "List")] public SwitchParameter List { get; set; }
        [Parameter(ParameterSetName = "List", Mandatory = true)] public string ProjectId { get; set; }
        [Parameter(ParameterSetName = "List")] public string CommonName { get; set; }
        [Parameter(ParameterSetName = "List")] public string FriendlyName { get; set; }
        [Parameter(ParameterSetName = "List")] public string Status { get; set; }
        [Parameter(ParameterSetName = "List")] public string[] CaId { get; set; }
        [Parameter(ParameterSetName = "List")] public int? Limit { get; set; }
        [Parameter(ParameterSetName = "List")] public int? Offset { get; set; }
        [Parameter(ParameterSetName = "List")] public SwitchParameter NoAutoPage { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                InfisicalConnection connection = InfisicalSessionManager.RequireCurrent();
                InfisicalPkiClient client = new InfisicalPkiClient(HttpClient, Logger);

                if (string.Equals(ParameterSetName, "Single", StringComparison.Ordinal))
                {
                    InfisicalCertificate cert = client.RetrieveCertificate(connection, SerialNumber);
                    if (cert != null)
                    {
                        WriteObject(cert);
                    }

                    return;
                }

                InfisicalCertificateSearchQuery query = new InfisicalCertificateSearchQuery
                {
                    ProjectId = ProjectId,
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
                ThrowTerminatingForException("GetInfisicalCertificateCmdlet", "GetCertificate", exception);
            }
        }
    }
}
