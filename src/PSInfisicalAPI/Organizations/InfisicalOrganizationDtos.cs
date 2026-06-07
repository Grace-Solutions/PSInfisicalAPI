using System.Collections.Generic;
using Newtonsoft.Json;

namespace PSInfisicalAPI.Organizations
{
    internal sealed class InfisicalOrganizationResponseDto
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("_id")] public string InternalId { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("slug")] public string Slug { get; set; }
        [JsonProperty("customerId")] public string CustomerId { get; set; }
        [JsonProperty("authEnforced")] public bool AuthEnforced { get; set; }
        [JsonProperty("scimEnabled")] public bool ScimEnabled { get; set; }
        [JsonProperty("createdAt")] public string CreatedAt { get; set; }
        [JsonProperty("updatedAt")] public string UpdatedAt { get; set; }
    }

    internal sealed class InfisicalOrganizationListResponseDto
    {
        [JsonProperty("organizations")] public List<InfisicalOrganizationResponseDto> Organizations { get; set; }
    }

    internal sealed class InfisicalOrganizationSingleResponseDto
    {
        [JsonProperty("organization")] public InfisicalOrganizationResponseDto Organization { get; set; }
    }

    internal sealed class InfisicalOrganizationCreateRequestDto
    {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("slug", NullValueHandling = NullValueHandling.Ignore)] public string Slug { get; set; }
    }

    internal sealed class InfisicalOrganizationUpdateRequestDto
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)] public string Name { get; set; }
        [JsonProperty("slug", NullValueHandling = NullValueHandling.Ignore)] public string Slug { get; set; }
    }
}
