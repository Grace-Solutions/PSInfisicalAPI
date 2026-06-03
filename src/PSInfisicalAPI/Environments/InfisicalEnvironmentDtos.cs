using System.Collections.Generic;
using Newtonsoft.Json;

namespace PSInfisicalAPI.Environments
{
    internal sealed class InfisicalEnvironmentResponseDto
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("_id")] public string InternalId { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("slug")] public string Slug { get; set; }
        [JsonProperty("position")] public int? Position { get; set; }
        [JsonProperty("projectId")] public string ProjectId { get; set; }
        [JsonProperty("workspaceId")] public string WorkspaceId { get; set; }
    }

    internal sealed class InfisicalEnvironmentSingleResponseDto
    {
        [JsonProperty("environment")] public InfisicalEnvironmentResponseDto Environment { get; set; }
    }

    internal sealed class InfisicalEnvironmentWorkspaceWrapperDto
    {
        [JsonProperty("workspace")] public InfisicalEnvironmentWorkspaceDto Workspace { get; set; }
        [JsonProperty("project")] public InfisicalEnvironmentWorkspaceDto Project { get; set; }
    }

    internal sealed class InfisicalEnvironmentWorkspaceDto
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("environments")] public List<InfisicalEnvironmentResponseDto> Environments { get; set; }
    }

    internal sealed class InfisicalEnvironmentCreateRequestDto
    {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("slug")] public string Slug { get; set; }
        [JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)] public int? Position { get; set; }
    }

    internal sealed class InfisicalEnvironmentUpdateRequestDto
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)] public string Name { get; set; }
        [JsonProperty("slug", NullValueHandling = NullValueHandling.Ignore)] public string Slug { get; set; }
        [JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)] public int? Position { get; set; }
    }
}
