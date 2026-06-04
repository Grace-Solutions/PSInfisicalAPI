using System;

namespace PSInfisicalAPI.Models
{
    public sealed class InfisicalCertificate
    {
        public string Id { get; set; }
        public string ProjectId { get; set; }
        public string CaId { get; set; }
        public string CaName { get; set; }
        public string CaCertId { get; set; }
        public string CertificateTemplateId { get; set; }
        public string ProfileId { get; set; }
        public string ProfileName { get; set; }
        public string ApplicationId { get; set; }
        public string ApplicationName { get; set; }
        public string PkiSubscriberId { get; set; }
        public string Status { get; set; }
        public string SerialNumber { get; set; }
        public string FriendlyName { get; set; }
        public string CommonName { get; set; }
        public string AltNames { get; set; }
        public string[] KeyUsages { get; set; }
        public string[] ExtendedKeyUsages { get; set; }
        public string KeyAlgorithm { get; set; }
        public string SignatureAlgorithm { get; set; }
        public string SubjectOrganization { get; set; }
        public string SubjectOrganizationalUnit { get; set; }
        public string SubjectCountry { get; set; }
        public string SubjectState { get; set; }
        public string SubjectLocality { get; set; }
        public string FingerprintSha256 { get; set; }
        public string FingerprintSha1 { get; set; }
        public bool? IsCA { get; set; }
        public int? PathLength { get; set; }
        public string Source { get; set; }
        public string EnrollmentType { get; set; }
        public bool HasPrivateKey { get; set; }
        public int? RevocationReason { get; set; }
        public string RenewalError { get; set; }
        public int? RenewBeforeDays { get; set; }
        public string RenewedFromCertificateId { get; set; }
        public string RenewedByCertificateId { get; set; }
        public DateTimeOffset? NotBeforeUtc { get; set; }
        public DateTimeOffset? NotAfterUtc { get; set; }
        public DateTimeOffset? RevokedAtUtc { get; set; }
        public DateTimeOffset? CreatedAtUtc { get; set; }
        public DateTimeOffset? UpdatedAtUtc { get; set; }

        public override string ToString()
        {
            return FriendlyName ?? CommonName ?? SerialNumber ?? Id;
        }
    }
}
