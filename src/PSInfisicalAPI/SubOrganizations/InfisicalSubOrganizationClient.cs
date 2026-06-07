using System;
using System.Collections.Generic;
using System.Globalization;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Endpoints;
using PSInfisicalAPI.Errors;
using PSInfisicalAPI.Http;
using PSInfisicalAPI.Logging;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Serialization;

namespace PSInfisicalAPI.SubOrganizations
{
    public sealed class InfisicalSubOrganizationClient
    {
        private const string Component = "SubOrganizationClient";

        private readonly IInfisicalLogger _logger;
        private readonly JsonInfisicalSerializer _serializer;
        private readonly InfisicalApiInvoker _invoker;

        public InfisicalSubOrganizationClient(IInfisicalHttpClient httpClient, IInfisicalLogger logger)
        {
            if (httpClient == null) { throw new ArgumentNullException(nameof(httpClient)); }
            _logger = logger ?? NullInfisicalLogger.Instance;
            _serializer = new JsonInfisicalSerializer();
            _invoker = new InfisicalApiInvoker(httpClient);
        }

        public InfisicalSubOrganization[] List(InfisicalConnection connection, int? limit, int? offset, string search, string orderBy, string orderDirection, bool? isAccessible)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }

            List<KeyValuePair<string, string>> queryParameters = new List<KeyValuePair<string, string>>();
            if (limit.HasValue) { queryParameters.Add(new KeyValuePair<string, string>("limit", limit.Value.ToString(CultureInfo.InvariantCulture))); }
            if (offset.HasValue) { queryParameters.Add(new KeyValuePair<string, string>("offset", offset.Value.ToString(CultureInfo.InvariantCulture))); }
            if (!string.IsNullOrEmpty(search)) { queryParameters.Add(new KeyValuePair<string, string>("search", search)); }
            if (!string.IsNullOrEmpty(orderBy)) { queryParameters.Add(new KeyValuePair<string, string>("orderBy", orderBy)); }
            if (!string.IsNullOrEmpty(orderDirection)) { queryParameters.Add(new KeyValuePair<string, string>("orderDirection", orderDirection)); }
            if (isAccessible.HasValue) { queryParameters.Add(new KeyValuePair<string, string>("isAccessible", isAccessible.Value ? "true" : "false")); }

            try
            {
                _logger.Information(Component, "Attempting to list Infisical sub-organizations. Please Wait...");
                InfisicalHttpResponse response = _invoker.Invoke(connection, InfisicalEndpointNames.ListSubOrganizations, "ListSubOrganizations", null, queryParameters, null);
                InfisicalSubOrganizationListResponseDto dto = _serializer.Deserialize<InfisicalSubOrganizationListResponseDto>(response.Body);
                response.Clear();

                InfisicalSubOrganization[] mapped = InfisicalSubOrganizationMapper.MapMany(dto != null ? dto.SubOrganizations : null);
                _logger.Information(Component, "Infisical sub-organization list retrieval was successful.");
                return mapped;
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical sub-organization list retrieval failed.");
                throw;
            }
        }

        public InfisicalSubOrganization Retrieve(InfisicalConnection connection, string subOrganizationId)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            if (string.IsNullOrEmpty(subOrganizationId)) { throw new InfisicalConfigurationException("SubOrganizationId is required."); }

            Dictionary<string, string> pathParameters = new Dictionary<string, string> { { "subOrgId", subOrganizationId } };

            try
            {
                _logger.Information(Component, string.Concat("Attempting to retrieve Infisical sub-organization '", subOrganizationId, "'. Please Wait..."));
                InfisicalHttpResponse response = _invoker.Invoke(connection, InfisicalEndpointNames.RetrieveSubOrganization, "RetrieveSubOrganization", pathParameters, null, null);
                InfisicalSubOrganizationSingleResponseDto dto = _serializer.Deserialize<InfisicalSubOrganizationSingleResponseDto>(response.Body);
                response.Clear();

                InfisicalSubOrganization mapped = InfisicalSubOrganizationMapper.Map(dto != null ? dto.SubOrganization : null);
                _logger.Information(Component, "Infisical sub-organization retrieval was successful.");
                return mapped;
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical sub-organization retrieval failed.");
                throw;
            }
        }

        public InfisicalSubOrganization Create(InfisicalConnection connection, string name, string slug)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            if (string.IsNullOrEmpty(name)) { throw new InfisicalConfigurationException("Name is required."); }
            if (string.IsNullOrEmpty(slug)) { throw new InfisicalConfigurationException("Slug is required."); }

            InfisicalSubOrganizationCreateRequestDto request = new InfisicalSubOrganizationCreateRequestDto { Name = name, Slug = slug };
            string body = _serializer.Serialize(request);

            try
            {
                _logger.Information(Component, string.Concat("Attempting to create Infisical sub-organization '", name, "'. Please Wait..."));
                InfisicalHttpResponse response = _invoker.Invoke(connection, InfisicalEndpointNames.CreateSubOrganization, "CreateSubOrganization", null, null, body);
                InfisicalSubOrganizationSingleResponseDto dto = _serializer.Deserialize<InfisicalSubOrganizationSingleResponseDto>(response.Body);
                response.Clear();

                InfisicalSubOrganization mapped = InfisicalSubOrganizationMapper.Map(dto != null ? dto.SubOrganization : null);
                _logger.Information(Component, "Infisical sub-organization creation was successful.");
                return mapped;
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical sub-organization creation failed.");
                throw;
            }
        }

        public InfisicalSubOrganization Update(InfisicalConnection connection, string subOrganizationId, string name, string slug)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            if (string.IsNullOrEmpty(subOrganizationId)) { throw new InfisicalConfigurationException("SubOrganizationId is required."); }

            Dictionary<string, string> pathParameters = new Dictionary<string, string> { { "subOrgId", subOrganizationId } };
            InfisicalSubOrganizationUpdateRequestDto request = new InfisicalSubOrganizationUpdateRequestDto { Name = name, Slug = slug };
            string body = _serializer.Serialize(request);

            try
            {
                _logger.Information(Component, string.Concat("Attempting to update Infisical sub-organization '", subOrganizationId, "'. Please Wait..."));
                InfisicalHttpResponse response = _invoker.Invoke(connection, InfisicalEndpointNames.UpdateSubOrganization, "UpdateSubOrganization", pathParameters, null, body);
                InfisicalSubOrganizationSingleResponseDto dto = _serializer.Deserialize<InfisicalSubOrganizationSingleResponseDto>(response.Body);
                response.Clear();

                InfisicalSubOrganization mapped = InfisicalSubOrganizationMapper.Map(dto != null ? dto.SubOrganization : null);
                _logger.Information(Component, "Infisical sub-organization update was successful.");
                return mapped;
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical sub-organization update failed.");
                throw;
            }
        }

        public void Delete(InfisicalConnection connection, string subOrganizationId)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            if (string.IsNullOrEmpty(subOrganizationId)) { throw new InfisicalConfigurationException("SubOrganizationId is required."); }

            Dictionary<string, string> pathParameters = new Dictionary<string, string> { { "subOrgId", subOrganizationId } };

            try
            {
                _logger.Information(Component, string.Concat("Attempting to delete Infisical sub-organization '", subOrganizationId, "'. Please Wait..."));
                InfisicalHttpResponse response = _invoker.Invoke(connection, InfisicalEndpointNames.DeleteSubOrganization, "DeleteSubOrganization", pathParameters, null, null);
                response.Clear();
                _logger.Information(Component, "Infisical sub-organization deletion was successful.");
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical sub-organization deletion failed.");
                throw;
            }
        }
    }
}
