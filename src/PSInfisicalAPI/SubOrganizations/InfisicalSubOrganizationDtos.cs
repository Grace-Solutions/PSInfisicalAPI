using System.Collections.Generic;
using Newtonsoft.Json;

namespace PSInfisicalAPI.SubOrganizations
{
    internal sealed class InfisicalSubOrganizationResponseDto
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("_id")] public string InternalId { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("slug")] public string Slug { get; set; }
        [JsonProperty("organizationId")] public string OrganizationId { get; set; }
        [JsonProperty("orgId")] public string OrgId { get; set; }
        [JsonProperty("isAccessible")] public bool IsAccessible { get; set; }
        [JsonProperty("createdAt")] public string CreatedAt { get; set; }
        [JsonProperty("updatedAt")] public string UpdatedAt { get; set; }
    }

    internal sealed class InfisicalSubOrganizationListResponseDto
    {
        [JsonProperty("subOrganizations")] public List<InfisicalSubOrganizationResponseDto> SubOrganizations { get; set; }
    }

    internal sealed class InfisicalSubOrganizationSingleResponseDto
    {
        [JsonProperty("subOrganization")] public InfisicalSubOrganizationResponseDto SubOrganization { get; set; }
    }

    internal sealed class InfisicalSubOrganizationCreateRequestDto
    {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("slug")] public string Slug { get; set; }
    }

    internal sealed class InfisicalSubOrganizationUpdateRequestDto
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)] public string Name { get; set; }
        [JsonProperty("slug", NullValueHandling = NullValueHandling.Ignore)] public string Slug { get; set; }
    }
}
