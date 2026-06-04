using System.Collections.Generic;
using Newtonsoft.Json;

namespace PSInfisicalAPI.Pki
{
    internal sealed class InfisicalCertificateResponseDto
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("projectId")] public string ProjectId { get; set; }
        [JsonProperty("caId")] public string CaId { get; set; }
        [JsonProperty("caName")] public string CaName { get; set; }
        [JsonProperty("caCertId")] public string CaCertId { get; set; }
        [JsonProperty("certificateTemplateId")] public string CertificateTemplateId { get; set; }
        [JsonProperty("profileId")] public string ProfileId { get; set; }
        [JsonProperty("profileName")] public string ProfileName { get; set; }
        [JsonProperty("applicationId")] public string ApplicationId { get; set; }
        [JsonProperty("applicationName")] public string ApplicationName { get; set; }
        [JsonProperty("pkiSubscriberId")] public string PkiSubscriberId { get; set; }
        [JsonProperty("status")] public string Status { get; set; }
        [JsonProperty("serialNumber")] public string SerialNumber { get; set; }
        [JsonProperty("friendlyName")] public string FriendlyName { get; set; }
        [JsonProperty("commonName")] public string CommonName { get; set; }
        [JsonProperty("altNames")] public string AltNames { get; set; }
        [JsonProperty("keyUsages")] public List<string> KeyUsages { get; set; }
        [JsonProperty("extendedKeyUsages")] public List<string> ExtendedKeyUsages { get; set; }
        [JsonProperty("keyAlgorithm")] public string KeyAlgorithm { get; set; }
        [JsonProperty("signatureAlgorithm")] public string SignatureAlgorithm { get; set; }
        [JsonProperty("subjectOrganization")] public string SubjectOrganization { get; set; }
        [JsonProperty("subjectOrganizationalUnit")] public string SubjectOrganizationalUnit { get; set; }
        [JsonProperty("subjectCountry")] public string SubjectCountry { get; set; }
        [JsonProperty("subjectState")] public string SubjectState { get; set; }
        [JsonProperty("subjectLocality")] public string SubjectLocality { get; set; }
        [JsonProperty("fingerprintSha256")] public string FingerprintSha256 { get; set; }
        [JsonProperty("fingerprintSha1")] public string FingerprintSha1 { get; set; }
        [JsonProperty("isCA")] public bool? IsCA { get; set; }
        [JsonProperty("pathLength")] public int? PathLength { get; set; }
        [JsonProperty("source")] public string Source { get; set; }
        [JsonProperty("enrollmentType")] public string EnrollmentType { get; set; }
        [JsonProperty("hasPrivateKey")] public bool HasPrivateKey { get; set; }
        [JsonProperty("revocationReason")] public int? RevocationReason { get; set; }
        [JsonProperty("renewalError")] public string RenewalError { get; set; }
        [JsonProperty("renewBeforeDays")] public int? RenewBeforeDays { get; set; }
        [JsonProperty("renewedFromCertificateId")] public string RenewedFromCertificateId { get; set; }
        [JsonProperty("renewedByCertificateId")] public string RenewedByCertificateId { get; set; }
        [JsonProperty("notBefore")] public string NotBefore { get; set; }
        [JsonProperty("notAfter")] public string NotAfter { get; set; }
        [JsonProperty("revokedAt")] public string RevokedAt { get; set; }
        [JsonProperty("createdAt")] public string CreatedAt { get; set; }
        [JsonProperty("updatedAt")] public string UpdatedAt { get; set; }
    }

    internal sealed class InfisicalCertificateSearchResponseDto
    {
        [JsonProperty("certificates")] public List<InfisicalCertificateResponseDto> Certificates { get; set; }
        [JsonProperty("totalCount")] public int TotalCount { get; set; }
    }

    internal sealed class InfisicalCertificateSingleResponseDto
    {
        [JsonProperty("certificate")] public InfisicalCertificateResponseDto Certificate { get; set; }
    }

    internal sealed class InfisicalCertificateSearchRequestDto
    {
        [JsonProperty("friendlyName", NullValueHandling = NullValueHandling.Ignore)] public string FriendlyName { get; set; }
        [JsonProperty("commonName", NullValueHandling = NullValueHandling.Ignore)] public string CommonName { get; set; }
        [JsonProperty("search", NullValueHandling = NullValueHandling.Ignore)] public string Search { get; set; }
        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)] public string Status { get; set; }
        [JsonProperty("offset", NullValueHandling = NullValueHandling.Ignore)] public int? Offset { get; set; }
        [JsonProperty("limit", NullValueHandling = NullValueHandling.Ignore)] public int? Limit { get; set; }
        [JsonProperty("caIds", NullValueHandling = NullValueHandling.Ignore)] public string[] CaIds { get; set; }
        [JsonProperty("profileIds", NullValueHandling = NullValueHandling.Ignore)] public string[] ProfileIds { get; set; }
        [JsonProperty("applicationIds", NullValueHandling = NullValueHandling.Ignore)] public string[] ApplicationIds { get; set; }
        [JsonProperty("enrollmentTypes", NullValueHandling = NullValueHandling.Ignore)] public string[] EnrollmentTypes { get; set; }
        [JsonProperty("extendedKeyUsage", NullValueHandling = NullValueHandling.Ignore)] public string ExtendedKeyUsage { get; set; }
        [JsonProperty("keyAlgorithm", NullValueHandling = NullValueHandling.Ignore)] public string[] KeyAlgorithm { get; set; }
        [JsonProperty("signatureAlgorithm", NullValueHandling = NullValueHandling.Ignore)] public string SignatureAlgorithm { get; set; }
        [JsonProperty("source", NullValueHandling = NullValueHandling.Ignore)] public string[] Source { get; set; }
        [JsonProperty("fromDate", NullValueHandling = NullValueHandling.Ignore)] public string FromDate { get; set; }
        [JsonProperty("toDate", NullValueHandling = NullValueHandling.Ignore)] public string ToDate { get; set; }
        [JsonProperty("notAfterFrom", NullValueHandling = NullValueHandling.Ignore)] public string NotAfterFrom { get; set; }
        [JsonProperty("notAfterTo", NullValueHandling = NullValueHandling.Ignore)] public string NotAfterTo { get; set; }
        [JsonProperty("notBeforeFrom", NullValueHandling = NullValueHandling.Ignore)] public string NotBeforeFrom { get; set; }
        [JsonProperty("notBeforeTo", NullValueHandling = NullValueHandling.Ignore)] public string NotBeforeTo { get; set; }
        [JsonProperty("sortBy", NullValueHandling = NullValueHandling.Ignore)] public string SortBy { get; set; }
        [JsonProperty("sortOrder", NullValueHandling = NullValueHandling.Ignore)] public string SortOrder { get; set; }
        [JsonProperty("forPkiSync", NullValueHandling = NullValueHandling.Ignore)] public bool? ForPkiSync { get; set; }
    }

    internal sealed class InfisicalCertificateBundleResponseDto
    {
        [JsonProperty("serialNumber")] public string SerialNumber { get; set; }
        [JsonProperty("certificate")] public string Certificate { get; set; }
        [JsonProperty("certificateChain")] public string CertificateChain { get; set; }
        [JsonProperty("privateKey")] public string PrivateKey { get; set; }
    }
}
