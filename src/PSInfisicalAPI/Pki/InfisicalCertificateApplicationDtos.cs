using System.Collections.Generic;
using Newtonsoft.Json;

namespace PSInfisicalAPI.Pki
{
    internal sealed class InfisicalCertificateApplicationResponseDto
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("projectId")] public string ProjectId { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("profileCount")] public int? ProfileCount { get; set; }
        [JsonProperty("memberCount")] public int? MemberCount { get; set; }
        [JsonProperty("certificateCount")] public int? CertificateCount { get; set; }
        [JsonProperty("createdAt")] public string CreatedAt { get; set; }
        [JsonProperty("updatedAt")] public string UpdatedAt { get; set; }
    }

    internal sealed class InfisicalCertificateApplicationListResponseDto
    {
        [JsonProperty("applications")] public List<InfisicalCertificateApplicationResponseDto> Applications { get; set; }
        [JsonProperty("total")] public int? Total { get; set; }
    }

    internal sealed class InfisicalCertificateApplicationProfileAttachmentDto
    {
        [JsonProperty("applicationId")] public string ApplicationId { get; set; }
        [JsonProperty("profileId")] public string ProfileId { get; set; }
        [JsonProperty("profileSlug")] public string ProfileSlug { get; set; }
        [JsonProperty("profileDescription")] public string ProfileDescription { get; set; }
        [JsonProperty("apiConfigId")] public string ApiConfigId { get; set; }
        [JsonProperty("estConfigId")] public string EstConfigId { get; set; }
        [JsonProperty("acmeConfigId")] public string AcmeConfigId { get; set; }
        [JsonProperty("scepConfigId")] public string ScepConfigId { get; set; }
        [JsonProperty("createdAt")] public string CreatedAt { get; set; }
        [JsonProperty("updatedAt")] public string UpdatedAt { get; set; }
    }

    internal sealed class InfisicalCertificateApplicationProfilesResponseDto
    {
        [JsonProperty("profiles")] public List<InfisicalCertificateApplicationProfileAttachmentDto> Profiles { get; set; }
    }

    internal sealed class InfisicalCertificateApplicationEnrollmentResponseDto
    {
        [JsonProperty("applicationId")] public string ApplicationId { get; set; }
        [JsonProperty("profileId")] public string ProfileId { get; set; }
        [JsonProperty("api")] public InfisicalCertificateApplicationApiEnrollmentDto Api { get; set; }
        [JsonProperty("est")] public InfisicalCertificateApplicationEstEnrollmentDto Est { get; set; }
        [JsonProperty("acme")] public InfisicalCertificateApplicationAcmeEnrollmentDto Acme { get; set; }
        [JsonProperty("scep")] public InfisicalCertificateApplicationScepEnrollmentDto Scep { get; set; }
        [JsonProperty("estConfigured")] public bool? EstConfigured { get; set; }
        [JsonProperty("acmeConfigured")] public bool? AcmeConfigured { get; set; }
        [JsonProperty("scepConfigured")] public bool? ScepConfigured { get; set; }
    }

    internal sealed class InfisicalCertificateApplicationApiEnrollmentDto
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("autoRenew")] public bool? AutoRenew { get; set; }
        [JsonProperty("renewBeforeDays")] public int? RenewBeforeDays { get; set; }
    }

    internal sealed class InfisicalCertificateApplicationEstEnrollmentDto
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("disableBootstrapCaValidation")] public bool? DisableBootstrapCaValidation { get; set; }
        [JsonProperty("estEndpointUrl")] public string EstEndpointUrl { get; set; }
    }

    internal sealed class InfisicalCertificateApplicationAcmeEnrollmentDto
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("skipDnsOwnershipVerification")] public bool? SkipDnsOwnershipVerification { get; set; }
        [JsonProperty("skipEabBinding")] public bool? SkipEabBinding { get; set; }
        [JsonProperty("directoryUrl")] public string DirectoryUrl { get; set; }
    }

    internal sealed class InfisicalCertificateApplicationScepEnrollmentDto
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("challengeType")] public string ChallengeType { get; set; }
        [JsonProperty("includeCaCertInResponse")] public bool? IncludeCaCertInResponse { get; set; }
        [JsonProperty("allowCertBasedRenewal")] public bool? AllowCertBasedRenewal { get; set; }
        [JsonProperty("dynamicChallengeExpiryMinutes")] public int? DynamicChallengeExpiryMinutes { get; set; }
        [JsonProperty("dynamicChallengeMaxPending")] public int? DynamicChallengeMaxPending { get; set; }
        [JsonProperty("scepEndpointUrl")] public string ScepEndpointUrl { get; set; }
        [JsonProperty("challengeEndpointUrl")] public string ChallengeEndpointUrl { get; set; }
        [JsonProperty("raCertificatePem")] public string RaCertificatePem { get; set; }
        [JsonProperty("raCertExpiresAt")] public string RaCertExpiresAt { get; set; }
    }
}
