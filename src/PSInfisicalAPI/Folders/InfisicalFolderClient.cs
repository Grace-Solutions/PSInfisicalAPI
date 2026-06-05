using System;
using System.Collections.Generic;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Endpoints;
using PSInfisicalAPI.Errors;
using PSInfisicalAPI.Http;
using PSInfisicalAPI.Logging;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Serialization;

namespace PSInfisicalAPI.Folders
{
    public sealed class InfisicalFolderClient
    {
        private const string Component = "FolderClient";

        private readonly IInfisicalLogger _logger;
        private readonly JsonInfisicalSerializer _serializer;
        private readonly InfisicalApiInvoker _invoker;

        public InfisicalFolderClient(IInfisicalHttpClient httpClient, IInfisicalLogger logger)
        {
            if (httpClient == null) { throw new ArgumentNullException(nameof(httpClient)); }
            _logger = logger ?? NullInfisicalLogger.Instance;
            _serializer = new JsonInfisicalSerializer();
            _invoker = new InfisicalApiInvoker(httpClient);
        }

        public InfisicalFolder[] List(InfisicalConnection connection, string projectId, string environment, string path)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            if (string.IsNullOrEmpty(projectId)) { throw new InfisicalConfigurationException("ProjectId is required."); }
            if (string.IsNullOrEmpty(environment)) { throw new InfisicalConfigurationException("Environment is required."); }
            string resolvedPath = FirstNonEmpty(path, "/");

            List<KeyValuePair<string, string>> queryParameters = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("workspaceId", projectId),
                new KeyValuePair<string, string>("environment", environment),
                new KeyValuePair<string, string>("path", resolvedPath)
            };

            try
            {
                _logger.Information(Component, "Attempting to list Infisical folders. Please Wait...");
                InfisicalHttpResponse response = _invoker.Invoke(connection, InfisicalEndpointNames.ListFolders, "ListFolders", null, queryParameters, null);
                InfisicalFolderListResponseDto dto = _serializer.Deserialize<InfisicalFolderListResponseDto>(response.Body);
                response.Clear();

                InfisicalFolder[] mapped = InfisicalFolderMapper.MapMany(dto != null ? dto.Folders : null, projectId, environment);
                _logger.Information(Component, "Infisical folder list retrieval was successful.");
                return mapped;
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical folder list retrieval failed.");
                throw;
            }
        }

        public InfisicalFolder Retrieve(InfisicalConnection connection, string projectId, string environment, string path, string folderNameOrId)
        {
            if (string.IsNullOrEmpty(folderNameOrId)) { throw new InfisicalConfigurationException("Folder name or id is required."); }

            InfisicalFolder[] all = List(connection, projectId, environment, path);
            foreach (InfisicalFolder folder in all)
            {
                if (string.Equals(folder.Id, folderNameOrId, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(folder.Name, folderNameOrId, StringComparison.OrdinalIgnoreCase))
                {
                    return folder;
                }
            }

            return null;
        }

        public InfisicalFolder Create(InfisicalConnection connection, string projectId, string environment, string name, string path)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            if (string.IsNullOrEmpty(projectId)) { throw new InfisicalConfigurationException("ProjectId is required."); }
            if (string.IsNullOrEmpty(environment)) { throw new InfisicalConfigurationException("Environment is required."); }
            if (string.IsNullOrEmpty(name)) { throw new InfisicalConfigurationException("Name is required."); }
            string resolvedPath = FirstNonEmpty(path, "/");

            InfisicalFolderCreateRequestDto request = new InfisicalFolderCreateRequestDto
            {
                WorkspaceId = projectId,
                Environment = environment,
                Name = name,
                Path = resolvedPath
            };
            string body = _serializer.Serialize(request);

            try
            {
                _logger.Information(Component, string.Concat("Attempting to create Infisical folder '", name, "'. Please Wait..."));
                InfisicalHttpResponse response = _invoker.Invoke(connection, InfisicalEndpointNames.CreateFolder, "CreateFolder", null, null, body);
                InfisicalFolderSingleResponseDto dto = _serializer.Deserialize<InfisicalFolderSingleResponseDto>(response.Body);
                response.Clear();

                InfisicalFolder mapped = InfisicalFolderMapper.Map(dto != null ? dto.Folder : null, projectId, environment);
                _logger.Information(Component, "Infisical folder creation was successful.");
                return mapped;
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical folder creation failed.");
                throw;
            }
        }

        public InfisicalFolder Update(InfisicalConnection connection, string projectId, string environment, string folderId, string name, string path)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            if (string.IsNullOrEmpty(projectId)) { throw new InfisicalConfigurationException("ProjectId is required."); }
            if (string.IsNullOrEmpty(environment)) { throw new InfisicalConfigurationException("Environment is required."); }
            if (string.IsNullOrEmpty(folderId)) { throw new InfisicalConfigurationException("FolderId is required."); }
            string resolvedPath = FirstNonEmpty(path, "/");
            if (string.IsNullOrEmpty(name)) { throw new InfisicalConfigurationException("Name is required."); }

            Dictionary<string, string> pathParameters = new Dictionary<string, string> { { "folderId", folderId } };
            InfisicalFolderUpdateRequestDto request = new InfisicalFolderUpdateRequestDto
            {
                WorkspaceId = projectId,
                Environment = environment,
                Name = name,
                Path = resolvedPath
            };
            string body = _serializer.Serialize(request);

            try
            {
                _logger.Information(Component, string.Concat("Attempting to update Infisical folder '", folderId, "'. Please Wait..."));
                InfisicalHttpResponse response = _invoker.Invoke(connection, InfisicalEndpointNames.UpdateFolder, "UpdateFolder", pathParameters, null, body);
                InfisicalFolderSingleResponseDto dto = _serializer.Deserialize<InfisicalFolderSingleResponseDto>(response.Body);
                response.Clear();

                InfisicalFolder mapped = InfisicalFolderMapper.Map(dto != null ? dto.Folder : null, projectId, environment);
                _logger.Information(Component, "Infisical folder update was successful.");
                return mapped;
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical folder update failed.");
                throw;
            }
        }

        public void Delete(InfisicalConnection connection, string projectId, string environment, string folderId, string path)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            if (string.IsNullOrEmpty(projectId)) { throw new InfisicalConfigurationException("ProjectId is required."); }
            if (string.IsNullOrEmpty(environment)) { throw new InfisicalConfigurationException("Environment is required."); }
            if (string.IsNullOrEmpty(folderId)) { throw new InfisicalConfigurationException("FolderId is required."); }
            string resolvedPath = FirstNonEmpty(path, "/");

            Dictionary<string, string> pathParameters = new Dictionary<string, string> { { "folderId", folderId } };
            List<KeyValuePair<string, string>> queryParameters = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("workspaceId", projectId),
                new KeyValuePair<string, string>("environment", environment),
                new KeyValuePair<string, string>("path", resolvedPath)
            };

            try
            {
                _logger.Information(Component, string.Concat("Attempting to delete Infisical folder '", folderId, "'. Please Wait..."));
                InfisicalHttpResponse response = _invoker.Invoke(connection, InfisicalEndpointNames.DeleteFolder, "DeleteFolder", pathParameters, queryParameters, null);
                response.Clear();
                _logger.Information(Component, "Infisical folder deletion was successful.");
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical folder deletion failed.");
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
