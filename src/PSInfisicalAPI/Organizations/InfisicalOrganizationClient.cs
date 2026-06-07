using System;
using System.Collections.Generic;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Endpoints;
using PSInfisicalAPI.Errors;
using PSInfisicalAPI.Http;
using PSInfisicalAPI.Logging;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Serialization;

namespace PSInfisicalAPI.Organizations
{
    public sealed class InfisicalOrganizationClient
    {
        private const string Component = "OrganizationClient";

        private readonly IInfisicalLogger _logger;
        private readonly JsonInfisicalSerializer _serializer;
        private readonly InfisicalApiInvoker _invoker;

        public InfisicalOrganizationClient(IInfisicalHttpClient httpClient, IInfisicalLogger logger)
        {
            if (httpClient == null) { throw new ArgumentNullException(nameof(httpClient)); }
            _logger = logger ?? NullInfisicalLogger.Instance;
            _serializer = new JsonInfisicalSerializer();
            _invoker = new InfisicalApiInvoker(httpClient);
        }

        public InfisicalOrganization[] List(InfisicalConnection connection)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }

            try
            {
                _logger.Information(Component, "Attempting to list Infisical organizations. Please Wait...");
                InfisicalHttpResponse response = _invoker.Invoke(connection, InfisicalEndpointNames.ListOrganizations, "ListOrganizations", null, null, null);
                InfisicalOrganizationListResponseDto dto = _serializer.Deserialize<InfisicalOrganizationListResponseDto>(response.Body);
                response.Clear();

                InfisicalOrganization[] mapped = InfisicalOrganizationMapper.MapMany(dto != null ? dto.Organizations : null);
                _logger.Information(Component, "Infisical organization list retrieval was successful.");
                return mapped;
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical organization list retrieval failed.");
                throw;
            }
        }

        public InfisicalOrganization Retrieve(InfisicalConnection connection, string organizationId)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            if (string.IsNullOrEmpty(organizationId)) { throw new InfisicalConfigurationException("OrganizationId is required."); }

            Dictionary<string, string> pathParameters = new Dictionary<string, string> { { "organizationId", organizationId } };

            try
            {
                _logger.Information(Component, string.Concat("Attempting to retrieve Infisical organization '", organizationId, "'. Please Wait..."));
                InfisicalHttpResponse response = _invoker.Invoke(connection, InfisicalEndpointNames.RetrieveOrganization, "RetrieveOrganization", pathParameters, null, null);
                InfisicalOrganizationSingleResponseDto dto = _serializer.Deserialize<InfisicalOrganizationSingleResponseDto>(response.Body);
                response.Clear();

                InfisicalOrganization mapped = InfisicalOrganizationMapper.Map(dto != null ? dto.Organization : null);
                _logger.Information(Component, "Infisical organization retrieval was successful.");
                return mapped;
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical organization retrieval failed.");
                throw;
            }
        }

        public InfisicalOrganization Create(InfisicalConnection connection, string name, string slug)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            if (string.IsNullOrEmpty(name)) { throw new InfisicalConfigurationException("Name is required."); }

            InfisicalOrganizationCreateRequestDto request = new InfisicalOrganizationCreateRequestDto { Name = name, Slug = slug };
            string body = _serializer.Serialize(request);

            try
            {
                _logger.Information(Component, string.Concat("Attempting to create Infisical organization '", name, "'. Please Wait..."));
                InfisicalHttpResponse response = _invoker.Invoke(connection, InfisicalEndpointNames.CreateOrganization, "CreateOrganization", null, null, body);
                InfisicalOrganizationSingleResponseDto dto = _serializer.Deserialize<InfisicalOrganizationSingleResponseDto>(response.Body);
                response.Clear();

                InfisicalOrganization mapped = InfisicalOrganizationMapper.Map(dto != null ? dto.Organization : null);
                _logger.Information(Component, "Infisical organization creation was successful.");
                return mapped;
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical organization creation failed.");
                throw;
            }
        }

        public InfisicalOrganization Update(InfisicalConnection connection, string organizationId, string name, string slug)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            if (string.IsNullOrEmpty(organizationId)) { throw new InfisicalConfigurationException("OrganizationId is required."); }

            Dictionary<string, string> pathParameters = new Dictionary<string, string> { { "organizationId", organizationId } };
            InfisicalOrganizationUpdateRequestDto request = new InfisicalOrganizationUpdateRequestDto { Name = name, Slug = slug };
            string body = _serializer.Serialize(request);

            try
            {
                _logger.Information(Component, string.Concat("Attempting to update Infisical organization '", organizationId, "'. Please Wait..."));
                InfisicalHttpResponse response = _invoker.Invoke(connection, InfisicalEndpointNames.UpdateOrganization, "UpdateOrganization", pathParameters, null, body);
                InfisicalOrganizationSingleResponseDto dto = _serializer.Deserialize<InfisicalOrganizationSingleResponseDto>(response.Body);
                response.Clear();

                InfisicalOrganization mapped = InfisicalOrganizationMapper.Map(dto != null ? dto.Organization : null);
                _logger.Information(Component, "Infisical organization update was successful.");
                return mapped;
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical organization update failed.");
                throw;
            }
        }

        public void Delete(InfisicalConnection connection, string organizationId)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            if (string.IsNullOrEmpty(organizationId)) { throw new InfisicalConfigurationException("OrganizationId is required."); }

            Dictionary<string, string> pathParameters = new Dictionary<string, string> { { "organizationId", organizationId } };

            try
            {
                _logger.Information(Component, string.Concat("Attempting to delete Infisical organization '", organizationId, "'. Please Wait..."));
                InfisicalHttpResponse response = _invoker.Invoke(connection, InfisicalEndpointNames.DeleteOrganization, "DeleteOrganization", pathParameters, null, null);
                response.Clear();
                _logger.Information(Component, "Infisical organization deletion was successful.");
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical organization deletion failed.");
                throw;
            }
        }
    }
}
