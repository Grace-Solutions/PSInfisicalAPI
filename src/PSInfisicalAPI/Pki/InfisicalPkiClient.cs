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

namespace PSInfisicalAPI.Pki
{
    public sealed class InfisicalPkiClient
    {
        private const string Component = "PkiClient";

        private readonly IInfisicalLogger _logger;
        private readonly JsonInfisicalSerializer _serializer;
        private readonly InfisicalApiInvoker _invoker;

        public InfisicalPkiClient(IInfisicalHttpClient httpClient, IInfisicalLogger logger)
        {
            if (httpClient == null) { throw new ArgumentNullException(nameof(httpClient)); }
            _logger = logger ?? NullInfisicalLogger.Instance;
            _serializer = new JsonInfisicalSerializer();
            _invoker = new InfisicalApiInvoker(httpClient);
        }

        public InfisicalCertificateAuthority[] ListInternalCertificateAuthorities(InfisicalConnection connection, string projectId)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            string resolvedProjectId = FirstNonEmpty(projectId, connection.ProjectId);

            List<KeyValuePair<string, string>> query = null;
            if (!string.IsNullOrEmpty(resolvedProjectId))
            {
                query = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("projectId", resolvedProjectId) };
            }

            try
            {
                _logger.Information(Component, "Attempting to list Infisical internal certificate authorities. Please Wait...");
                InfisicalHttpResponse response = _invoker.InvokeWithCandidateFallback(connection, InfisicalEndpointNames.ListInternalCertificateAuthorities, "ListInternalCertificateAuthorities", null, query, null);
                InfisicalInternalCaListResponseDto dto = _serializer.Deserialize<InfisicalInternalCaListResponseDto>(response.Body);
                response.Clear();

                List<InfisicalInternalCaResponseDto> source = dto != null ? (dto.CertificateAuthorities ?? dto.Cas) : null;
                InfisicalCertificateAuthority[] mapped = InfisicalCaMapper.MapMany(source, resolvedProjectId);
                _logger.Information(Component, "Infisical internal certificate authority list retrieval was successful.");
                return mapped;
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical internal certificate authority list retrieval failed.");
                throw;
            }
        }

        public InfisicalCertificateAuthority GetInternalCertificateAuthority(InfisicalConnection connection, string caId, string projectId)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            if (string.IsNullOrEmpty(caId)) { throw new InfisicalConfigurationException("CaId is required."); }

            Dictionary<string, string> pathParameters = new Dictionary<string, string> { { "caId", caId } };

            try
            {
                _logger.Information(Component, string.Concat("Attempting to retrieve Infisical internal certificate authority '", caId, "'. Please Wait..."));
                InfisicalHttpResponse response = _invoker.InvokeWithCandidateFallback(connection, InfisicalEndpointNames.RetrieveInternalCertificateAuthority, "RetrieveInternalCertificateAuthority", pathParameters, null, null);
                InfisicalInternalCaSingleResponseDto dto = _serializer.Deserialize<InfisicalInternalCaSingleResponseDto>(response.Body);
                response.Clear();

                InfisicalInternalCaResponseDto inner = dto != null ? (dto.CertificateAuthority ?? dto.Ca) : null;
                if (inner == null)
                {
                    inner = _serializer.Deserialize<InfisicalInternalCaResponseDto>(response.Body);
                }

                InfisicalCertificateAuthority mapped = InfisicalCaMapper.Map(inner, FirstNonEmpty(projectId, connection.ProjectId));
                _logger.Information(Component, "Infisical internal certificate authority retrieval was successful.");
                return mapped;
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical internal certificate authority retrieval failed.");
                throw;
            }
        }

        public InfisicalCertificateSearchResult SearchCertificates(InfisicalConnection connection, InfisicalCertificateSearchQuery query)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            if (query == null) { throw new ArgumentNullException(nameof(query)); }
            string resolvedProjectId = FirstNonEmpty(query.ProjectId, connection.ProjectId);
            if (string.IsNullOrEmpty(resolvedProjectId)) { throw new InfisicalConfigurationException("ProjectId is required."); }

            Dictionary<string, string> pathParameters = new Dictionary<string, string> { { "projectId", resolvedProjectId } };
            InfisicalCertificateSearchRequestDto request = BuildSearchRequest(query);
            string body = _serializer.Serialize(request);

            try
            {
                _logger.Information(Component, "Attempting to search Infisical certificates. Please Wait...");
                InfisicalHttpResponse response = _invoker.InvokeWithCandidateFallback(connection, InfisicalEndpointNames.SearchCertificates, "SearchCertificates", pathParameters, null, body);
                InfisicalCertificateSearchResponseDto dto = _serializer.Deserialize<InfisicalCertificateSearchResponseDto>(response.Body);
                response.Clear();

                InfisicalCertificate[] mapped = InfisicalCertificateMapper.MapMany(dto != null ? dto.Certificates : null, resolvedProjectId);
                int total = dto != null ? dto.TotalCount : mapped.Length;
                _logger.Information(Component, "Infisical certificate search was successful.");
                return new InfisicalCertificateSearchResult { Certificates = mapped, TotalCount = total };
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical certificate search failed.");
                throw;
            }
        }

        public InfisicalCertificateBundle GetCertificateBundle(InfisicalConnection connection, string serialNumber)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            if (string.IsNullOrEmpty(serialNumber)) { throw new InfisicalConfigurationException("SerialNumber is required."); }

            Dictionary<string, string> pathParameters = new Dictionary<string, string> { { "serialNumber", serialNumber } };

            try
            {
                _logger.Information(Component, string.Concat("Attempting to retrieve Infisical certificate bundle for '", serialNumber, "'. Please Wait..."));
                InfisicalHttpResponse response = _invoker.InvokeWithCandidateFallback(connection, InfisicalEndpointNames.GetCertificateBundle, "GetCertificateBundle", pathParameters, null, null);
                InfisicalCertificateBundleResponseDto dto = _serializer.Deserialize<InfisicalCertificateBundleResponseDto>(response.Body);
                response.Clear();

                InfisicalCertificateBundle mapped = InfisicalCertificateMapper.MapBundle(dto);
                _logger.Information(Component, "Infisical certificate bundle retrieval was successful.");
                return mapped;
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical certificate bundle retrieval failed.");
                throw;
            }
        }

        internal static InfisicalCertificateSearchRequestDto BuildSearchRequest(InfisicalCertificateSearchQuery query)
        {
            return new InfisicalCertificateSearchRequestDto
            {
                FriendlyName = query.FriendlyName,
                CommonName = query.CommonName,
                Search = query.Search,
                Status = query.Status,
                Offset = query.Offset,
                Limit = query.Limit,
                CaIds = query.CaIds,
                ProfileIds = query.ProfileIds,
                ApplicationIds = query.ApplicationIds,
                EnrollmentTypes = query.EnrollmentTypes,
                ExtendedKeyUsage = query.ExtendedKeyUsage,
                KeyAlgorithm = query.KeyAlgorithm,
                SignatureAlgorithm = query.SignatureAlgorithm,
                Source = query.Source,
                FromDate = FormatTimestamp(query.FromDate),
                ToDate = FormatTimestamp(query.ToDate),
                NotAfterFrom = FormatTimestamp(query.NotAfterFrom),
                NotAfterTo = FormatTimestamp(query.NotAfterTo),
                NotBeforeFrom = FormatTimestamp(query.NotBeforeFrom),
                NotBeforeTo = FormatTimestamp(query.NotBeforeTo),
                SortBy = query.SortBy,
                SortOrder = query.SortOrder,
                ForPkiSync = query.ForPkiSync
            };
        }

        private static string FormatTimestamp(DateTimeOffset? value)
        {
            if (!value.HasValue) { return null; }
            return value.Value.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
        }

        private static string FirstNonEmpty(params string[] values)
        {
            if (values == null) { return null; }
            foreach (string value in values) { if (!string.IsNullOrEmpty(value)) { return value; } }
            return null;
        }
    }

    public sealed class InfisicalCertificateSearchResult
    {
        public InfisicalCertificate[] Certificates { get; set; }
        public int TotalCount { get; set; }
    }
}
