using System.Collections.Generic;
using Newtonsoft.Json;

namespace PSInfisicalAPI.Pki
{
    internal sealed class InfisicalPkiSubscriberResponseDto
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("projectId")] public string ProjectId { get; set; }
        [JsonProperty("caId")] public string CaId { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("commonName")] public string CommonName { get; set; }
        [JsonProperty("status")] public string Status { get; set; }
        [JsonProperty("ttl")] public string Ttl { get; set; }
        [JsonProperty("subjectAlternativeNames")] public List<string> SubjectAlternativeNames { get; set; }
        [JsonProperty("keyUsages")] public List<string> KeyUsages { get; set; }
        [JsonProperty("extendedKeyUsages")] public List<string> ExtendedKeyUsages { get; set; }
        [JsonProperty("enableAutoRenewal")] public bool? EnableAutoRenewal { get; set; }
        [JsonProperty("autoRenewalPeriodInDays")] public int? AutoRenewalPeriodInDays { get; set; }
        [JsonProperty("lastOperationStatus")] public string LastOperationStatus { get; set; }
        [JsonProperty("lastOperationMessage")] public string LastOperationMessage { get; set; }
        [JsonProperty("lastOperationAt")] public string LastOperationAt { get; set; }
        [JsonProperty("createdAt")] public string CreatedAt { get; set; }
        [JsonProperty("updatedAt")] public string UpdatedAt { get; set; }
        [JsonProperty("properties")] public InfisicalPkiSubscriberPropertiesDto Properties { get; set; }
    }

    internal sealed class InfisicalPkiSubscriberPropertiesDto
    {
        [JsonProperty("azureTemplateType")] public string AzureTemplateType { get; set; }
        [JsonProperty("organization")] public string Organization { get; set; }
        [JsonProperty("organizationalUnit")] public string OrganizationalUnit { get; set; }
        [JsonProperty("country")] public string Country { get; set; }
        [JsonProperty("state")] public string State { get; set; }
        [JsonProperty("locality")] public string Locality { get; set; }
        [JsonProperty("emailAddress")] public string EmailAddress { get; set; }
    }

    internal sealed class InfisicalPkiSubscriberListResponseDto
    {
        [JsonProperty("subscribers")] public List<InfisicalPkiSubscriberResponseDto> Subscribers { get; set; }
    }
}
