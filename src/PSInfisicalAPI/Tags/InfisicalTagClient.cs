using System;
using System.Collections.Generic;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Endpoints;
using PSInfisicalAPI.Errors;
using PSInfisicalAPI.Http;
using PSInfisicalAPI.Logging;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Serialization;

namespace PSInfisicalAPI.Tags
{
    public sealed class InfisicalTagClient
    {
        private const string Component = "TagClient";

        private readonly IInfisicalLogger _logger;
        private readonly JsonInfisicalSerializer _serializer;
        private readonly InfisicalApiInvoker _invoker;

        public InfisicalTagClient(IInfisicalHttpClient httpClient, IInfisicalLogger logger)
        {
            if (httpClient == null) { throw new ArgumentNullException(nameof(httpClient)); }
            _logger = logger ?? NullInfisicalLogger.Instance;
            _serializer = new JsonInfisicalSerializer();
            _invoker = new InfisicalApiInvoker(httpClient);
        }

        public InfisicalTag[] List(InfisicalConnection connection, string projectId)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            string resolvedProjectId = FirstNonEmpty(projectId, connection.ProjectId);
            if (string.IsNullOrEmpty(resolvedProjectId)) { throw new InfisicalConfigurationException("ProjectId is required."); }

            Dictionary<string, string> pathParameters = new Dictionary<string, string> { { "projectId", resolvedProjectId } };

            try
            {
                _logger.Information(Component, "Attempting to list Infisical tags. Please Wait...");
                InfisicalHttpResponse response = _invoker.Invoke(connection, InfisicalEndpointNames.ListTags, "ListTags", pathParameters, null, null);
                InfisicalTagListResponseDto dto = _serializer.Deserialize<InfisicalTagListResponseDto>(response.Body);
                response.Clear();

                List<InfisicalTagResponseDto> source = dto != null ? (dto.WorkspaceTags ?? dto.Tags) : null;
                InfisicalTag[] mapped = InfisicalTagMapper.MapMany(source, resolvedProjectId);
                _logger.Information(Component, "Infisical tag list retrieval was successful.");
                return mapped;
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical tag list retrieval failed.");
                throw;
            }
        }

        public InfisicalTag Retrieve(InfisicalConnection connection, string projectId, string tagSlugOrId)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            string resolvedProjectId = FirstNonEmpty(projectId, connection.ProjectId);
            if (string.IsNullOrEmpty(resolvedProjectId)) { throw new InfisicalConfigurationException("ProjectId is required."); }
            if (string.IsNullOrEmpty(tagSlugOrId)) { throw new InfisicalConfigurationException("Tag slug or id is required."); }

            InfisicalTag[] all = List(connection, resolvedProjectId);
            foreach (InfisicalTag tag in all)
            {
                if (string.Equals(tag.Id, tagSlugOrId, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(tag.Slug, tagSlugOrId, StringComparison.OrdinalIgnoreCase))
                {
                    return tag;
                }
            }

            return null;
        }

        public InfisicalTag Create(InfisicalConnection connection, string projectId, string slug, string name, string color)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            string resolvedProjectId = FirstNonEmpty(projectId, connection.ProjectId);
            if (string.IsNullOrEmpty(resolvedProjectId)) { throw new InfisicalConfigurationException("ProjectId is required."); }
            if (string.IsNullOrEmpty(slug)) { throw new InfisicalConfigurationException("Slug is required."); }

            Dictionary<string, string> pathParameters = new Dictionary<string, string> { { "projectId", resolvedProjectId } };
            InfisicalTagCreateRequestDto request = new InfisicalTagCreateRequestDto { Slug = slug, Name = name, Color = color };
            string body = _serializer.Serialize(request);

            try
            {
                _logger.Information(Component, string.Concat("Attempting to create Infisical tag '", slug, "'. Please Wait..."));
                InfisicalHttpResponse response = _invoker.Invoke(connection, InfisicalEndpointNames.CreateTag, "CreateTag", pathParameters, null, body);
                InfisicalTagSingleResponseDto dto = _serializer.Deserialize<InfisicalTagSingleResponseDto>(response.Body);
                response.Clear();

                InfisicalTagResponseDto inner = dto != null ? (dto.WorkspaceTag ?? dto.Tag) : null;
                InfisicalTag mapped = InfisicalTagMapper.Map(inner, resolvedProjectId);
                _logger.Information(Component, "Infisical tag creation was successful.");
                return mapped;
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical tag creation failed.");
                throw;
            }
        }

        public InfisicalTag Update(InfisicalConnection connection, string projectId, string tagId, string slug, string name, string color)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            string resolvedProjectId = FirstNonEmpty(projectId, connection.ProjectId);
            if (string.IsNullOrEmpty(resolvedProjectId)) { throw new InfisicalConfigurationException("ProjectId is required."); }
            if (string.IsNullOrEmpty(tagId)) { throw new InfisicalConfigurationException("TagId is required."); }

            Dictionary<string, string> pathParameters = new Dictionary<string, string> { { "projectId", resolvedProjectId }, { "tagId", tagId } };
            InfisicalTagUpdateRequestDto request = new InfisicalTagUpdateRequestDto { Slug = slug, Name = name, Color = color };
            string body = _serializer.Serialize(request);

            try
            {
                _logger.Information(Component, string.Concat("Attempting to update Infisical tag '", tagId, "'. Please Wait..."));
                InfisicalHttpResponse response = _invoker.Invoke(connection, InfisicalEndpointNames.UpdateTag, "UpdateTag", pathParameters, null, body);
                InfisicalTagSingleResponseDto dto = _serializer.Deserialize<InfisicalTagSingleResponseDto>(response.Body);
                response.Clear();

                InfisicalTagResponseDto inner = dto != null ? (dto.WorkspaceTag ?? dto.Tag) : null;
                InfisicalTag mapped = InfisicalTagMapper.Map(inner, resolvedProjectId);
                _logger.Information(Component, "Infisical tag update was successful.");
                return mapped;
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical tag update failed.");
                throw;
            }
        }

        public void Delete(InfisicalConnection connection, string projectId, string tagId)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            string resolvedProjectId = FirstNonEmpty(projectId, connection.ProjectId);
            if (string.IsNullOrEmpty(resolvedProjectId)) { throw new InfisicalConfigurationException("ProjectId is required."); }
            if (string.IsNullOrEmpty(tagId)) { throw new InfisicalConfigurationException("TagId is required."); }

            Dictionary<string, string> pathParameters = new Dictionary<string, string> { { "projectId", resolvedProjectId }, { "tagId", tagId } };

            try
            {
                _logger.Information(Component, string.Concat("Attempting to delete Infisical tag '", tagId, "'. Please Wait..."));
                InfisicalHttpResponse response = _invoker.Invoke(connection, InfisicalEndpointNames.DeleteTag, "DeleteTag", pathParameters, null, null);
                response.Clear();
                _logger.Information(Component, "Infisical tag deletion was successful.");
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical tag deletion failed.");
                throw;
            }
        }

        private static string FirstNonEmpty(params string[] values)
        {
            if (values == null) { return null; }
            foreach (string value in values) { if (!string.IsNullOrEmpty(value)) { return value; } }
            return null;
        }
    }
}
