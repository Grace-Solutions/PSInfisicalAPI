using System.Collections.Generic;
using Newtonsoft.Json;

namespace PSInfisicalAPI.Pki
{
    internal sealed class InfisicalInternalCaConfigurationDto
    {
        [JsonProperty("type")] public string Type { get; set; }
        [JsonProperty("friendlyName")] public string FriendlyName { get; set; }
        [JsonProperty("commonName")] public string CommonName { get; set; }
        [JsonProperty("organization")] public string OrganizationName { get; set; }
        [JsonProperty("ou")] public string OrganizationUnit { get; set; }
        [JsonProperty("country")] public string Country { get; set; }
        [JsonProperty("province")] public string State { get; set; }
        [JsonProperty("locality")] public string Locality { get; set; }
        [JsonProperty("notBefore")] public string NotBefore { get; set; }
        [JsonProperty("notAfter")] public string NotAfter { get; set; }
        [JsonProperty("maxPathLength")] public int? MaxPathLength { get; set; }
        [JsonProperty("keyAlgorithm")] public string KeyAlgorithm { get; set; }
        [JsonProperty("dn")] public string DistinguishedName { get; set; }
        [JsonProperty("parentCaId")] public string ParentCaId { get; set; }
        [JsonProperty("serialNumber")] public string SerialNumber { get; set; }
        [JsonProperty("activeCaCertId")] public string ActiveCaCertId { get; set; }
    }

    internal sealed class InfisicalInternalCaResponseDto
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("projectId")] public string ProjectId { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("friendlyName")] public string FriendlyName { get; set; }
        [JsonProperty("type")] public string Type { get; set; }
        [JsonProperty("status")] public string Status { get; set; }
        [JsonProperty("enableDirectIssuance")] public bool? EnableDirectIssuance { get; set; }
        [JsonProperty("keyAlgorithm")] public string KeyAlgorithm { get; set; }
        [JsonProperty("dn")] public string DistinguishedName { get; set; }
        [JsonProperty("organization")] public string OrganizationName { get; set; }
        [JsonProperty("ou")] public string OrganizationUnit { get; set; }
        [JsonProperty("country")] public string Country { get; set; }
        [JsonProperty("province")] public string State { get; set; }
        [JsonProperty("locality")] public string Locality { get; set; }
        [JsonProperty("commonName")] public string CommonName { get; set; }
        [JsonProperty("maxPathLength")] public int? MaxPathLength { get; set; }
        [JsonProperty("notBefore")] public string NotBefore { get; set; }
        [JsonProperty("notAfter")] public string NotAfter { get; set; }
        [JsonProperty("serialNumber")] public string SerialNumber { get; set; }
        [JsonProperty("parentCaId")] public string ParentCaId { get; set; }
        [JsonProperty("activeCaCertId")] public string ActiveCaCertId { get; set; }
        [JsonProperty("createdAt")] public string CreatedAt { get; set; }
        [JsonProperty("updatedAt")] public string UpdatedAt { get; set; }
        [JsonProperty("configuration")] public InfisicalInternalCaConfigurationDto Configuration { get; set; }
    }

    internal sealed class InfisicalInternalCaListResponseDto
    {
        [JsonProperty("certificateAuthorities")] public List<InfisicalInternalCaResponseDto> CertificateAuthorities { get; set; }
        [JsonProperty("cas")] public List<InfisicalInternalCaResponseDto> Cas { get; set; }
    }

    internal sealed class InfisicalInternalCaSingleResponseDto
    {
        [JsonProperty("certificateAuthority")] public InfisicalInternalCaResponseDto CertificateAuthority { get; set; }
        [JsonProperty("ca")] public InfisicalInternalCaResponseDto Ca { get; set; }
    }
}
