using System.Collections.Generic;
using Newtonsoft.Json;

namespace PSInfisicalAPI.Pki
{
    internal sealed class InfisicalSignCertificateBySubscriberRequestDto
    {
        [JsonProperty("projectId")] public string ProjectId { get; set; }
        [JsonProperty("csr")] public string Csr { get; set; }
    }

    internal sealed class InfisicalSignCertificateByCaRequestDto
    {
        [JsonProperty("csr")] public string Csr { get; set; }
        [JsonProperty("commonName", NullValueHandling = NullValueHandling.Ignore)] public string CommonName { get; set; }
        [JsonProperty("altNames", NullValueHandling = NullValueHandling.Ignore)] public string AltNames { get; set; }
        [JsonProperty("ttl", NullValueHandling = NullValueHandling.Ignore)] public string Ttl { get; set; }
        [JsonProperty("notBefore", NullValueHandling = NullValueHandling.Ignore)] public string NotBefore { get; set; }
        [JsonProperty("notAfter", NullValueHandling = NullValueHandling.Ignore)] public string NotAfter { get; set; }
        [JsonProperty("friendlyName", NullValueHandling = NullValueHandling.Ignore)] public string FriendlyName { get; set; }
        [JsonProperty("pkiCollectionId", NullValueHandling = NullValueHandling.Ignore)] public string PkiCollectionId { get; set; }
        [JsonProperty("keyUsages", NullValueHandling = NullValueHandling.Ignore)] public List<string> KeyUsages { get; set; }
        [JsonProperty("extendedKeyUsages", NullValueHandling = NullValueHandling.Ignore)] public List<string> ExtendedKeyUsages { get; set; }
    }

    internal sealed class InfisicalSignCertificateResponseDto
    {
        [JsonProperty("certificate")] public string Certificate { get; set; }
        [JsonProperty("certificateChain")] public string CertificateChain { get; set; }
        [JsonProperty("issuingCaCertificate")] public string IssuingCaCertificate { get; set; }
        [JsonProperty("serialNumber")] public string SerialNumber { get; set; }
    }
}
