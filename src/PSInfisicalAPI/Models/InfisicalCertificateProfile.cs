using System;

namespace PSInfisicalAPI.Models
{
    public sealed class InfisicalCertificateProfile
    {
        public string Id { get; set; }
        public string ProjectId { get; set; }
        public string CaId { get; set; }
        public string CertificatePolicyId { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public string EnrollmentType { get; set; }
        public string IssuerType { get; set; }
        public string EstConfigId { get; set; }
        public string ApiConfigId { get; set; }
        public string AcmeConfigId { get; set; }
        public string ScepConfigId { get; set; }
        public DateTimeOffset? CreatedAtUtc { get; set; }
        public DateTimeOffset? UpdatedAtUtc { get; set; }
        public InfisicalCertificateProfileDefaults Defaults { get; set; }
        public InfisicalCertificateAuthoritySummary CertificateAuthority { get; set; }
        public InfisicalCertificatePolicySummary CertificatePolicy { get; set; }
        public InfisicalCertificateProfileApiConfig ApiConfig { get; set; }
    }

    public sealed class InfisicalCertificateProfileDefaults
    {
        public int? TtlDays { get; set; }
        public string KeyAlgorithm { get; set; }
        public string SignatureAlgorithm { get; set; }
        public string[] KeyUsages { get; set; }
        public string[] ExtendedKeyUsages { get; set; }
    }

    public sealed class InfisicalCertificateAuthoritySummary
    {
        public string Id { get; set; }
        public string Status { get; set; }
        public string Name { get; set; }
        public bool? IsExternal { get; set; }
        public string ExternalType { get; set; }
    }

    public sealed class InfisicalCertificatePolicySummary
    {
        public string Id { get; set; }
        public string ProjectId { get; set; }
        public string Name { get; set; }
    }

    public sealed class InfisicalCertificateProfileApiConfig
    {
        public string Id { get; set; }
        public bool? AutoRenew { get; set; }
        public int? RenewBeforeDays { get; set; }
    }
}
