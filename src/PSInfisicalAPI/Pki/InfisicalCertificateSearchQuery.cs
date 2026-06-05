using System;
using System.Collections.Generic;

namespace PSInfisicalAPI.Pki
{
    public sealed class InfisicalCertificateSearchQuery
    {
        public string ProjectId { get; set; }
        public string FriendlyName { get; set; }
        public string CommonName { get; set; }
        public string Search { get; set; }
        public string Status { get; set; }
        public int? Offset { get; set; }
        public int? Limit { get; set; }
        public string[] CaIds { get; set; }
        public string[] ProfileIds { get; set; }
        public string ApplicationId { get; set; }
        public string[] ApplicationIds { get; set; }
        public string[] EnrollmentTypes { get; set; }
        public string ExtendedKeyUsage { get; set; }
        public string[] KeyAlgorithm { get; set; }
        public string SignatureAlgorithm { get; set; }
        public int[] KeySizes { get; set; }
        public string[] Source { get; set; }
        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }
        public DateTimeOffset? NotAfterFrom { get; set; }
        public DateTimeOffset? NotAfterTo { get; set; }
        public DateTimeOffset? NotBeforeFrom { get; set; }
        public DateTimeOffset? NotBeforeTo { get; set; }
        public IDictionary<string, string> Metadata { get; set; }
        public string SortBy { get; set; }
        public string SortOrder { get; set; }
        public bool? ForPkiSync { get; set; }
    }
}
