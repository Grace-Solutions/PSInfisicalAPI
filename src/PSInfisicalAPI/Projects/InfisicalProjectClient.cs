using System;
using System.Collections.Generic;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Endpoints;
using PSInfisicalAPI.Errors;
using PSInfisicalAPI.Http;
using PSInfisicalAPI.Logging;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Serialization;

namespace PSInfisicalAPI.Projects
{
    public sealed class InfisicalProjectClient
    {
        private const string Component = "ProjectClient";

        private readonly IInfisicalLogger _logger;
        private readonly JsonInfisicalSerializer _serializer;
        private readonly InfisicalApiInvoker _invoker;

        public InfisicalProjectClient(IInfisicalHttpClient httpClient, IInfisicalLogger logger)
        {
            if (httpClient == null) { throw new ArgumentNullException(nameof(httpClient)); }
            _logger = logger ?? NullInfisicalLogger.Instance;
            _serializer = new JsonInfisicalSerializer();
            _invoker = new InfisicalApiInvoker(httpClient);
        }

        public InfisicalProject[] List(InfisicalConnection connection)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }

            try
            {
                _logger.Information(Component, "Attempting to list Infisical projects. Please Wait...");
                InfisicalHttpResponse response = _invoker.Invoke(connection, InfisicalEndpointNames.ListProjects, "ListProjects", null, null, null);
                InfisicalProjectListResponseDto dto = _serializer.Deserialize<InfisicalProjectListResponseDto>(response.Body);
                response.Clear();

                List<InfisicalProjectResponseDto> source = (dto != null && dto.Workspaces != null) ? dto.Workspaces : (dto != null ? dto.Projects : null);
                InfisicalProject[] mapped = InfisicalProjectMapper.MapMany(source);
                _logger.Information(Component, "Infisical project list retrieval was successful.");
                return mapped;
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical project list retrieval failed.");
                throw;
            }
        }

        public InfisicalProject Retrieve(InfisicalConnection connection, string projectId)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            if (string.IsNullOrEmpty(projectId)) { throw new InfisicalConfigurationException("ProjectId is required."); }

            Dictionary<string, string> pathParameters = new Dictionary<string, string> { { "projectId", projectId } };

            try
            {
                _logger.Information(Component, string.Concat("Attempting to retrieve Infisical project '", projectId, "'. Please Wait..."));
                InfisicalHttpResponse response = _invoker.Invoke(connection, InfisicalEndpointNames.RetrieveProject, "RetrieveProject", pathParameters, null, null);
                InfisicalProjectSingleResponseDto dto = _serializer.Deserialize<InfisicalProjectSingleResponseDto>(response.Body);
                response.Clear();

                InfisicalProjectResponseDto inner = dto != null ? (dto.Workspace ?? dto.Project) : null;
                InfisicalProject mapped = InfisicalProjectMapper.Map(inner);
                _logger.Information(Component, "Infisical project retrieval was successful.");
                return mapped;
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical project retrieval failed.");
                throw;
            }
        }

        public InfisicalProject Create(InfisicalConnection connection, string projectName, string slug, string description, string type, string organizationId)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            if (string.IsNullOrEmpty(projectName)) { throw new InfisicalConfigurationException("ProjectName is required."); }

            InfisicalProjectCreateRequestDto request = new InfisicalProjectCreateRequestDto
            {
                ProjectName = projectName,
                Slug = slug,
                ProjectDescription = description,
                Type = type,
                OrganizationId = organizationId
            };

            string body = _serializer.Serialize(request);

            try
            {
                _logger.Information(Component, string.Concat("Attempting to create Infisical project '", projectName, "'. Please Wait..."));
                InfisicalHttpResponse response = _invoker.Invoke(connection, InfisicalEndpointNames.CreateProject, "CreateProject", null, null, body);
                InfisicalProjectSingleResponseDto dto = _serializer.Deserialize<InfisicalProjectSingleResponseDto>(response.Body);
                response.Clear();

                InfisicalProjectResponseDto inner = dto != null ? (dto.Project ?? dto.Workspace) : null;
                InfisicalProject mapped = InfisicalProjectMapper.Map(inner);
                _logger.Information(Component, "Infisical project creation was successful.");
                return mapped;
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical project creation failed.");
                throw;
            }
        }

        public InfisicalProject Update(InfisicalConnection connection, string projectId, string name, string description, bool? autoCapitalization)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            if (string.IsNullOrEmpty(projectId)) { throw new InfisicalConfigurationException("ProjectId is required."); }

            Dictionary<string, string> pathParameters = new Dictionary<string, string> { { "projectId", projectId } };
            InfisicalProjectUpdateRequestDto request = new InfisicalProjectUpdateRequestDto { Name = name, Description = description, AutoCapitalization = autoCapitalization };
            string body = _serializer.Serialize(request);

            try
            {
                _logger.Information(Component, string.Concat("Attempting to update Infisical project '", projectId, "'. Please Wait..."));
                InfisicalHttpResponse response = _invoker.Invoke(connection, InfisicalEndpointNames.UpdateProject, "UpdateProject", pathParameters, null, body);
                InfisicalProjectSingleResponseDto dto = _serializer.Deserialize<InfisicalProjectSingleResponseDto>(response.Body);
                response.Clear();

                InfisicalProjectResponseDto inner = dto != null ? (dto.Workspace ?? dto.Project) : null;
                InfisicalProject mapped = InfisicalProjectMapper.Map(inner);
                _logger.Information(Component, "Infisical project update was successful.");
                return mapped;
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical project update failed.");
                throw;
            }
        }

        public void Delete(InfisicalConnection connection, string projectId)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            if (string.IsNullOrEmpty(projectId)) { throw new InfisicalConfigurationException("ProjectId is required."); }

            Dictionary<string, string> pathParameters = new Dictionary<string, string> { { "projectId", projectId } };

            try
            {
                _logger.Information(Component, string.Concat("Attempting to delete Infisical project '", projectId, "'. Please Wait..."));
                InfisicalHttpResponse response = _invoker.Invoke(connection, InfisicalEndpointNames.DeleteProject, "DeleteProject", pathParameters, null, null);
                response.Clear();
                _logger.Information(Component, "Infisical project deletion was successful.");
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical project deletion failed.");
                throw;
            }
        }
    }
}
