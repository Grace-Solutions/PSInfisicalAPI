using System;
using System.Collections.Generic;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Endpoints;
using PSInfisicalAPI.Errors;
using PSInfisicalAPI.Http;
using PSInfisicalAPI.Logging;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Serialization;

namespace PSInfisicalAPI.Environments
{
    public sealed class InfisicalEnvironmentClient
    {
        private const string Component = "EnvironmentClient";

        private readonly IInfisicalLogger _logger;
        private readonly JsonInfisicalSerializer _serializer;
        private readonly InfisicalApiInvoker _invoker;

        public InfisicalEnvironmentClient(IInfisicalHttpClient httpClient, IInfisicalLogger logger)
        {
            if (httpClient == null) { throw new ArgumentNullException(nameof(httpClient)); }
            _logger = logger ?? NullInfisicalLogger.Instance;
            _serializer = new JsonInfisicalSerializer();
            _invoker = new InfisicalApiInvoker(httpClient);
        }

        public InfisicalEnvironment[] List(InfisicalConnection connection, string projectId)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            if (string.IsNullOrEmpty(projectId)) { throw new InfisicalConfigurationException("ProjectId is required."); }

            Dictionary<string, string> pathParameters = new Dictionary<string, string> { { "projectId", projectId } };

            try
            {
                _logger.Information(Component, "Attempting to list Infisical environments. Please Wait...");
                InfisicalHttpResponse response = _invoker.Invoke(connection, InfisicalEndpointNames.ListEnvironments, "ListEnvironments", pathParameters, null, null);
                InfisicalEnvironmentWorkspaceWrapperDto dto = _serializer.Deserialize<InfisicalEnvironmentWorkspaceWrapperDto>(response.Body);
                response.Clear();

                InfisicalEnvironmentWorkspaceDto workspace = dto != null ? (dto.Workspace ?? dto.Project) : null;
                List<InfisicalEnvironmentResponseDto> envs = workspace != null ? workspace.Environments : null;
                InfisicalEnvironment[] mapped = InfisicalEnvironmentMapper.MapMany(envs, projectId);
                _logger.Information(Component, "Infisical environment list retrieval was successful.");
                return mapped;
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical environment list retrieval failed.");
                throw;
            }
        }

        public InfisicalEnvironment Retrieve(InfisicalConnection connection, string projectId, string environmentSlugOrId)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            if (string.IsNullOrEmpty(projectId)) { throw new InfisicalConfigurationException("ProjectId is required."); }
            if (string.IsNullOrEmpty(environmentSlugOrId)) { throw new InfisicalConfigurationException("Environment is required."); }

            InfisicalEnvironment[] all = List(connection, projectId);
            foreach (InfisicalEnvironment env in all)
            {
                if (string.Equals(env.Id, environmentSlugOrId, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(env.Slug, environmentSlugOrId, StringComparison.OrdinalIgnoreCase))
                {
                    return env;
                }
            }

            return null;
        }

        public InfisicalEnvironment Create(InfisicalConnection connection, string projectId, string name, string slug, int? position)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            if (string.IsNullOrEmpty(projectId)) { throw new InfisicalConfigurationException("ProjectId is required."); }
            if (string.IsNullOrEmpty(name)) { throw new InfisicalConfigurationException("Name is required."); }
            if (string.IsNullOrEmpty(slug)) { throw new InfisicalConfigurationException("Slug is required."); }

            Dictionary<string, string> pathParameters = new Dictionary<string, string> { { "projectId", projectId } };
            InfisicalEnvironmentCreateRequestDto request = new InfisicalEnvironmentCreateRequestDto { Name = name, Slug = slug, Position = position };
            string body = _serializer.Serialize(request);

            try
            {
                _logger.Information(Component, string.Concat("Attempting to create Infisical environment '", slug, "'. Please Wait..."));
                InfisicalHttpResponse response = _invoker.Invoke(connection, InfisicalEndpointNames.CreateEnvironment, "CreateEnvironment", pathParameters, null, body);
                InfisicalEnvironmentSingleResponseDto dto = _serializer.Deserialize<InfisicalEnvironmentSingleResponseDto>(response.Body);
                response.Clear();

                InfisicalEnvironment mapped = InfisicalEnvironmentMapper.Map(dto != null ? dto.Environment : null, projectId);
                _logger.Information(Component, "Infisical environment creation was successful.");
                return mapped;
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical environment creation failed.");
                throw;
            }
        }

        public InfisicalEnvironment Update(InfisicalConnection connection, string projectId, string environmentId, string name, string slug, int? position)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            if (string.IsNullOrEmpty(projectId)) { throw new InfisicalConfigurationException("ProjectId is required."); }
            if (string.IsNullOrEmpty(environmentId)) { throw new InfisicalConfigurationException("EnvironmentId is required."); }

            Dictionary<string, string> pathParameters = new Dictionary<string, string> { { "projectId", projectId }, { "environmentId", environmentId } };
            InfisicalEnvironmentUpdateRequestDto request = new InfisicalEnvironmentUpdateRequestDto { Name = name, Slug = slug, Position = position };
            string body = _serializer.Serialize(request);

            try
            {
                _logger.Information(Component, string.Concat("Attempting to update Infisical environment '", environmentId, "'. Please Wait..."));
                InfisicalHttpResponse response = _invoker.Invoke(connection, InfisicalEndpointNames.UpdateEnvironment, "UpdateEnvironment", pathParameters, null, body);
                InfisicalEnvironmentSingleResponseDto dto = _serializer.Deserialize<InfisicalEnvironmentSingleResponseDto>(response.Body);
                response.Clear();

                InfisicalEnvironment mapped = InfisicalEnvironmentMapper.Map(dto != null ? dto.Environment : null, projectId);
                _logger.Information(Component, "Infisical environment update was successful.");
                return mapped;
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical environment update failed.");
                throw;
            }
        }

        public void Delete(InfisicalConnection connection, string projectId, string environmentId)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            if (string.IsNullOrEmpty(projectId)) { throw new InfisicalConfigurationException("ProjectId is required."); }
            if (string.IsNullOrEmpty(environmentId)) { throw new InfisicalConfigurationException("EnvironmentId is required."); }

            Dictionary<string, string> pathParameters = new Dictionary<string, string> { { "projectId", projectId }, { "environmentId", environmentId } };

            try
            {
                _logger.Information(Component, string.Concat("Attempting to delete Infisical environment '", environmentId, "'. Please Wait..."));
                InfisicalHttpResponse response = _invoker.Invoke(connection, InfisicalEndpointNames.DeleteEnvironment, "DeleteEnvironment", pathParameters, null, null);
                response.Clear();
                _logger.Information(Component, "Infisical environment deletion was successful.");
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical environment deletion failed.");
                throw;
            }
        }

    }
}
