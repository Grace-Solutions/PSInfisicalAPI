using System;

namespace PSInfisicalAPI.Models
{
    public sealed class InfisicalCertificateAuthority
    {
        public string Id { get; set; }
        public string ProjectId { get; set; }
        public string Name { get; set; }
        public string FriendlyName { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public bool? EnableDirectIssuance { get; set; }
        public string KeyAlgorithm { get; set; }
        public string DistinguishedName { get; set; }
        public string OrganizationName { get; set; }
        public string OrganizationUnit { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string Locality { get; set; }
        public string CommonName { get; set; }
        public int? MaxPathLength { get; set; }
        public string NotBefore { get; set; }
        public string NotAfter { get; set; }
        public string SerialNumber { get; set; }
        public string ParentCaId { get; set; }
        public string ActiveCaCertId { get; set; }
        public DateTimeOffset? CreatedAtUtc { get; set; }
        public DateTimeOffset? UpdatedAtUtc { get; set; }

        public override string ToString()
        {
            return FriendlyName ?? Name ?? CommonName ?? Id;
        }
    }
}
