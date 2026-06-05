using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PSInfisicalAPI.Pki
{
    internal sealed class InfisicalCertificatePolicyResponseDto
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("projectId")] public string ProjectId { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("subject")] public InfisicalCertificatePolicySubjectDto Subject { get; set; }
        [JsonProperty("sans")] public JToken SansRaw { get; set; }
        [JsonProperty("keyUsages")] public InfisicalCertificatePolicyUsagesDto KeyUsages { get; set; }
        [JsonProperty("extendedKeyUsages")] public InfisicalCertificatePolicyUsagesDto ExtendedKeyUsages { get; set; }
        [JsonProperty("algorithms")] public InfisicalCertificatePolicyAlgorithmsDto Algorithms { get; set; }
        [JsonProperty("validity")] public InfisicalCertificatePolicyValidityDto Validity { get; set; }
        [JsonProperty("createdAt")] public string CreatedAt { get; set; }
        [JsonProperty("updatedAt")] public string UpdatedAt { get; set; }
    }

    internal sealed class InfisicalCertificatePolicySubjectDto
    {
        [JsonProperty("type")] public string Type { get; set; }
        [JsonProperty("allowed")] public JToken AllowedRaw { get; set; }
    }

    internal sealed class InfisicalCertificatePolicySanDto
    {
        [JsonProperty("type")] public string Type { get; set; }
        [JsonProperty("allowed")] public JToken AllowedRaw { get; set; }
        [JsonProperty("required")] public JToken RequiredRaw { get; set; }
    }

    internal sealed class InfisicalCertificatePolicyUsagesDto
    {
        [JsonProperty("allowed")] public JToken AllowedRaw { get; set; }
        [JsonProperty("required")] public JToken RequiredRaw { get; set; }
    }

    internal sealed class InfisicalCertificatePolicyAlgorithmsDto
    {
        [JsonProperty("signature")] public string Signature { get; set; }
        [JsonProperty("keyAlgorithm")] public JToken KeyAlgorithmRaw { get; set; }
    }

    internal sealed class InfisicalCertificatePolicyValidityDto
    {
        [JsonProperty("max")] public string Max { get; set; }
    }

    internal sealed class InfisicalCertificatePolicyListResponseDto
    {
        [JsonProperty("certificatePolicies")] public List<InfisicalCertificatePolicyResponseDto> CertificatePolicies { get; set; }
        [JsonProperty("totalCount")] public int? TotalCount { get; set; }
    }
}
