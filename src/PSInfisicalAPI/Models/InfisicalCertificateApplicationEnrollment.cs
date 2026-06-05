using System;

namespace PSInfisicalAPI.Models
{
    public sealed class InfisicalCertificateApplicationEnrollment
    {
        public string ApplicationId { get; set; }
        public string ProfileId { get; set; }
        public InfisicalCertificateApplicationApiEnrollment Api { get; set; }
        public InfisicalCertificateApplicationEstEnrollment Est { get; set; }
        public InfisicalCertificateApplicationAcmeEnrollment Acme { get; set; }
        public InfisicalCertificateApplicationScepEnrollment Scep { get; set; }
        public bool ApiConfigured { get { return Api != null; } }
        public bool EstConfigured { get; set; }
        public bool AcmeConfigured { get; set; }
        public bool ScepConfigured { get; set; }
    }

    public sealed class InfisicalCertificateApplicationApiEnrollment
    {
        public string Id { get; set; }
        public bool? AutoRenew { get; set; }
        public int? RenewBeforeDays { get; set; }
    }

    public sealed class InfisicalCertificateApplicationEstEnrollment
    {
        public string Id { get; set; }
        public bool? DisableBootstrapCaValidation { get; set; }
        public string EstEndpointUrl { get; set; }
    }

    public sealed class InfisicalCertificateApplicationAcmeEnrollment
    {
        public string Id { get; set; }
        public bool? SkipDnsOwnershipVerification { get; set; }
        public bool? SkipEabBinding { get; set; }
        public string DirectoryUrl { get; set; }
    }

    public sealed class InfisicalCertificateApplicationScepEnrollment
    {
        public string Id { get; set; }
        public string ChallengeType { get; set; }
        public bool? IncludeCaCertInResponse { get; set; }
        public bool? AllowCertBasedRenewal { get; set; }
        public int? DynamicChallengeExpiryMinutes { get; set; }
        public int? DynamicChallengeMaxPending { get; set; }
        public string ScepEndpointUrl { get; set; }
        public string ChallengeEndpointUrl { get; set; }
        public string RaCertificatePem { get; set; }
        public string RaCertificateThumbprint { get; set; }
        public DateTimeOffset? RaCertExpiresAtUtc { get; set; }
    }
}
