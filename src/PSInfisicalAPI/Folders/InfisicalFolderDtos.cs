using System.Collections.Generic;
using Newtonsoft.Json;

namespace PSInfisicalAPI.Folders
{
    internal sealed class InfisicalFolderResponseDto
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("_id")] public string InternalId { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("path")] public string Path { get; set; }
        [JsonProperty("parentId")] public string ParentId { get; set; }
        [JsonProperty("envId")] public string EnvId { get; set; }
        [JsonProperty("environment")] public string Environment { get; set; }
        [JsonProperty("projectId")] public string ProjectId { get; set; }
        [JsonProperty("workspaceId")] public string WorkspaceId { get; set; }
        [JsonProperty("createdAt")] public string CreatedAt { get; set; }
        [JsonProperty("updatedAt")] public string UpdatedAt { get; set; }
    }

    internal sealed class InfisicalFolderListResponseDto
    {
        [JsonProperty("folders")] public List<InfisicalFolderResponseDto> Folders { get; set; }
    }

    internal sealed class InfisicalFolderSingleResponseDto
    {
        [JsonProperty("folder")] public InfisicalFolderResponseDto Folder { get; set; }
    }

    internal sealed class InfisicalFolderCreateRequestDto
    {
        [JsonProperty("workspaceId")] public string WorkspaceId { get; set; }
        [JsonProperty("environment")] public string Environment { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("path", NullValueHandling = NullValueHandling.Ignore)] public string Path { get; set; }
        [JsonProperty("directory", NullValueHandling = NullValueHandling.Ignore)] public string Directory { get; set; }
    }

    internal sealed class InfisicalFolderUpdateRequestDto
    {
        [JsonProperty("workspaceId")] public string WorkspaceId { get; set; }
        [JsonProperty("environment")] public string Environment { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("path", NullValueHandling = NullValueHandling.Ignore)] public string Path { get; set; }
    }
}
