using System;
using System.Collections;
using System.Collections.Generic;
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

        [Parameter(ParameterSetName = "List", Mandatory = true)] public string ProjectId { get; set; }
        [Parameter(ParameterSetName = "List")] public string CommonName { get; set; }
        [Parameter(ParameterSetName = "List")] public string FriendlyName { get; set; }
        [Parameter(ParameterSetName = "List")] public string Search { get; set; }
        [Parameter(ParameterSetName = "List")] public string Status { get; set; }
        [Parameter(ParameterSetName = "List")] public string[] CaId { get; set; }
        [Parameter(ParameterSetName = "List")] public string[] ProfileId { get; set; }
        [Parameter(ParameterSetName = "List")] public string ApplicationId { get; set; }
        [Parameter(ParameterSetName = "List")] public string[] ApplicationIds { get; set; }
        [Parameter(ParameterSetName = "List")] public string[] EnrollmentType { get; set; }
        [Parameter(ParameterSetName = "List")] public string ExtendedKeyUsage { get; set; }
        [Parameter(ParameterSetName = "List")] public string[] KeyAlgorithm { get; set; }
        [Parameter(ParameterSetName = "List")] public string SignatureAlgorithm { get; set; }
        [Parameter(ParameterSetName = "List")] public int[] KeySize { get; set; }
        [Parameter(ParameterSetName = "List")] public string[] Source { get; set; }
        [Parameter(ParameterSetName = "List")] public DateTimeOffset? FromDate { get; set; }
        [Parameter(ParameterSetName = "List")] public DateTimeOffset? ToDate { get; set; }
        [Parameter(ParameterSetName = "List")] public DateTimeOffset? NotAfterFrom { get; set; }
        [Parameter(ParameterSetName = "List")] public DateTimeOffset? NotAfterTo { get; set; }
        [Parameter(ParameterSetName = "List")] public DateTimeOffset? NotBeforeFrom { get; set; }
        [Parameter(ParameterSetName = "List")] public DateTimeOffset? NotBeforeTo { get; set; }
        [Parameter(ParameterSetName = "List")] public Hashtable Metadata { get; set; }
        [Parameter(ParameterSetName = "List")] public SwitchParameter ForPkiSync { get; set; }

        [Parameter(ParameterSetName = "List")]
        [ValidateSet("notAfter", "notBefore", "createdAt", "commonName", "keyAlgorithm", "status")]
        public string SortBy { get; set; }

        [Parameter(ParameterSetName = "List")]
        [ValidateSet("asc", "desc")]
        public string SortOrder { get; set; }

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
                    Search = Search,
                    Status = Status,
                    CaIds = CaId,
                    ProfileIds = ProfileId,
                    ApplicationId = ApplicationId,
                    ApplicationIds = ApplicationIds,
                    EnrollmentTypes = EnrollmentType,
                    ExtendedKeyUsage = ExtendedKeyUsage,
                    KeyAlgorithm = KeyAlgorithm,
                    SignatureAlgorithm = SignatureAlgorithm,
                    KeySizes = KeySize,
                    Source = Source,
                    FromDate = FromDate,
                    ToDate = ToDate,
                    NotAfterFrom = NotAfterFrom,
                    NotAfterTo = NotAfterTo,
                    NotBeforeFrom = NotBeforeFrom,
                    NotBeforeTo = NotBeforeTo,
                    Metadata = ToStringDictionary(Metadata),
                    SortBy = SortBy,
                    SortOrder = SortOrder,
                    ForPkiSync = ForPkiSync.IsPresent ? (bool?)true : null,
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

        private static Dictionary<string, string> ToStringDictionary(Hashtable hashtable)
        {
            if (hashtable == null) { return null; }
            Dictionary<string, string> result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (DictionaryEntry entry in hashtable)
            {
                if (entry.Key == null) { continue; }
                result[entry.Key.ToString()] = entry.Value != null ? entry.Value.ToString() : null;
            }
            return result;
        }
    }
}
