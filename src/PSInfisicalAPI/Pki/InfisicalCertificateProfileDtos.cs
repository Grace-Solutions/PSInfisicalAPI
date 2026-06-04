using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PSInfisicalAPI.Pki
{
    internal sealed class InfisicalCertificateProfileResponseDto
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("projectId")] public string ProjectId { get; set; }
        [JsonProperty("caId")] public string CaId { get; set; }
        [JsonProperty("certificatePolicyId")] public string CertificatePolicyId { get; set; }
        [JsonProperty("slug")] public string Slug { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("enrollmentType")] public string EnrollmentType { get; set; }
        [JsonProperty("issuerType")] public string IssuerType { get; set; }
        [JsonProperty("estConfigId")] public string EstConfigId { get; set; }
        [JsonProperty("apiConfigId")] public string ApiConfigId { get; set; }
        [JsonProperty("acmeConfigId")] public string AcmeConfigId { get; set; }
        [JsonProperty("scepConfigId")] public string ScepConfigId { get; set; }
        [JsonProperty("createdAt")] public string CreatedAt { get; set; }
        [JsonProperty("updatedAt")] public string UpdatedAt { get; set; }
        [JsonProperty("defaults")] public InfisicalCertificateProfileDefaultsDto Defaults { get; set; }
        [JsonProperty("certificateAuthority")] public InfisicalCertificateAuthoritySummaryDto CertificateAuthority { get; set; }
        [JsonProperty("certificatePolicy")] public InfisicalCertificatePolicySummaryDto CertificatePolicy { get; set; }
        [JsonProperty("apiConfig")] public InfisicalCertificateProfileApiConfigDto ApiConfig { get; set; }
    }

    internal sealed class InfisicalCertificateProfileDefaultsDto
    {
        [JsonProperty("ttlDays")] public int? TtlDays { get; set; }
        [JsonProperty("keyAlgorithm")] public string KeyAlgorithm { get; set; }
        [JsonProperty("signatureAlgorithm")] public string SignatureAlgorithm { get; set; }
        [JsonProperty("keyUsages")] public JToken KeyUsagesRaw { get; set; }
        [JsonProperty("extendedKeyUsages")] public JToken ExtendedKeyUsagesRaw { get; set; }
    }

    internal sealed class InfisicalCertificateAuthoritySummaryDto
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("status")] public string Status { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("isExternal")] public bool? IsExternal { get; set; }
        [JsonProperty("externalType")] public string ExternalType { get; set; }
    }

    internal sealed class InfisicalCertificatePolicySummaryDto
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("projectId")] public string ProjectId { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
    }

    internal sealed class InfisicalCertificateProfileApiConfigDto
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("autoRenew")] public bool? AutoRenew { get; set; }
        [JsonProperty("renewBeforeDays")] public int? RenewBeforeDays { get; set; }
    }

    internal sealed class InfisicalCertificateProfileListResponseDto
    {
        [JsonProperty("certificateProfiles")] public List<InfisicalCertificateProfileResponseDto> CertificateProfiles { get; set; }
        [JsonProperty("totalCount")] public int? TotalCount { get; set; }
    }
}
