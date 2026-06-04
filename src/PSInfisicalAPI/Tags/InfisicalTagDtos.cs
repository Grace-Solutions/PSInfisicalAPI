using System.Collections.Generic;
using Newtonsoft.Json;

namespace PSInfisicalAPI.Tags
{
    internal sealed class InfisicalTagResponseDto
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("_id")] public string InternalId { get; set; }
        [JsonProperty("slug")] public string Slug { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("color")] public string Color { get; set; }
        [JsonProperty("projectId")] public string ProjectId { get; set; }
        [JsonProperty("workspaceId")] public string WorkspaceId { get; set; }
        [JsonProperty("createdAt")] public string CreatedAt { get; set; }
        [JsonProperty("updatedAt")] public string UpdatedAt { get; set; }
    }

    internal sealed class InfisicalTagListResponseDto
    {
        [JsonProperty("workspaceTags")] public List<InfisicalTagResponseDto> WorkspaceTags { get; set; }
        [JsonProperty("tags")] public List<InfisicalTagResponseDto> Tags { get; set; }
    }

    internal sealed class InfisicalTagSingleResponseDto
    {
        [JsonProperty("workspaceTag")] public InfisicalTagResponseDto WorkspaceTag { get; set; }
        [JsonProperty("tag")] public InfisicalTagResponseDto Tag { get; set; }
    }

    internal sealed class InfisicalTagCreateRequestDto
    {
        [JsonProperty("slug")] public string Slug { get; set; }
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)] public string Name { get; set; }
        [JsonProperty("color", NullValueHandling = NullValueHandling.Ignore)] public string Color { get; set; }
    }

    internal sealed class InfisicalTagUpdateRequestDto
    {
        [JsonProperty("slug", NullValueHandling = NullValueHandling.Ignore)] public string Slug { get; set; }
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)] public string Name { get; set; }
        [JsonProperty("color", NullValueHandling = NullValueHandling.Ignore)] public string Color { get; set; }
    }
}
