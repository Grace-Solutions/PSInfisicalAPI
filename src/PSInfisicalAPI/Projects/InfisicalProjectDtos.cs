using System.Collections.Generic;
using Newtonsoft.Json;

namespace PSInfisicalAPI.Projects
{
    internal sealed class InfisicalProjectResponseDto
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("_id")] public string InternalId { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("slug")] public string Slug { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("organization")] public string Organization { get; set; }
        [JsonProperty("orgId")] public string OrgId { get; set; }
        [JsonProperty("type")] public string Type { get; set; }
        [JsonProperty("autoCapitalization")] public bool AutoCapitalization { get; set; }
        [JsonProperty("createdAt")] public string CreatedAt { get; set; }
        [JsonProperty("updatedAt")] public string UpdatedAt { get; set; }
        [JsonProperty("environments")] public List<InfisicalProjectEnvironmentDto> Environments { get; set; }
    }

    internal sealed class InfisicalProjectEnvironmentDto
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("slug")] public string Slug { get; set; }
    }

    internal sealed class InfisicalProjectListResponseDto
    {
        [JsonProperty("workspaces")] public List<InfisicalProjectResponseDto> Workspaces { get; set; }
        [JsonProperty("projects")] public List<InfisicalProjectResponseDto> Projects { get; set; }
    }

    internal sealed class InfisicalProjectSingleResponseDto
    {
        [JsonProperty("workspace")] public InfisicalProjectResponseDto Workspace { get; set; }
        [JsonProperty("project")] public InfisicalProjectResponseDto Project { get; set; }
    }

    internal sealed class InfisicalProjectCreateRequestDto
    {
        [JsonProperty("projectName")] public string ProjectName { get; set; }
        [JsonProperty("slug", NullValueHandling = NullValueHandling.Ignore)] public string Slug { get; set; }
        [JsonProperty("projectDescription", NullValueHandling = NullValueHandling.Ignore)] public string ProjectDescription { get; set; }
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)] public string Type { get; set; }
        [JsonProperty("organizationId", NullValueHandling = NullValueHandling.Ignore)] public string OrganizationId { get; set; }
    }

    internal sealed class InfisicalProjectUpdateRequestDto
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)] public string Name { get; set; }
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)] public string Description { get; set; }
        [JsonProperty("autoCapitalization", NullValueHandling = NullValueHandling.Ignore)] public bool? AutoCapitalization { get; set; }
    }
}
