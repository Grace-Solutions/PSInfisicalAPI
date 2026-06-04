using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Linq;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Endpoints;
using PSInfisicalAPI.Errors;
using PSInfisicalAPI.Http;
using PSInfisicalAPI.Logging;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Serialization;
using System.Linq;

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

            List<KeyValuePair<string, string>> query = null;
            if (!string.IsNullOrEmpty(projectId))
            {
                query = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("projectId", projectId) };
            }

            try
            {
                _logger.Information(Component, "Attempting to list Infisical internal certificate authorities. Please Wait...");
                InfisicalHttpResponse response = _invoker.InvokeWithCandidateFallback(connection, InfisicalEndpointNames.ListInternalCertificateAuthorities, "ListInternalCertificateAuthorities", null, query, null);
                string body = response.Body;
                response.Clear();

                List<InfisicalInternalCaResponseDto> source = ParseCaListBody(body);
                string fallbackProjectId = !string.IsNullOrEmpty(projectId) ? projectId : connection.ProjectId;
                InfisicalCertificateAuthority[] mapped = InfisicalCaMapper.MapMany(source, fallbackProjectId);
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
            List<KeyValuePair<string, string>> query = null;
            if (!string.IsNullOrEmpty(projectId))
            {
                query = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("projectId", projectId) };
            }

            try
            {
                _logger.Information(Component, string.Concat("Attempting to retrieve Infisical internal certificate authority '", caId, "'. Please Wait..."));
                InfisicalHttpResponse response = _invoker.InvokeWithCandidateFallback(connection, InfisicalEndpointNames.RetrieveInternalCertificateAuthority, "RetrieveInternalCertificateAuthority", pathParameters, query, null);
                string body = response.Body;
                response.Clear();

                InfisicalInternalCaResponseDto inner = ParseCaSingleBody(body);
                string fallbackProjectId = !string.IsNullOrEmpty(projectId) ? projectId : connection.ProjectId;
                InfisicalCertificateAuthority mapped = InfisicalCaMapper.Map(inner, fallbackProjectId);
                _logger.Information(Component, "Infisical internal certificate authority retrieval was successful.");
                return mapped;
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical internal certificate authority retrieval failed.");
                throw;
            }
        }

        public InfisicalCertificate RetrieveCertificate(InfisicalConnection connection, string identifier)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            if (string.IsNullOrEmpty(identifier)) { throw new InfisicalConfigurationException("Identifier (serial number or id) is required."); }

            Dictionary<string, string> pathParameters = new Dictionary<string, string> { { "serialNumber", identifier } };

            try
            {
                _logger.Information(Component, string.Concat("Attempting to retrieve Infisical certificate '", identifier, "'. Please Wait..."));
                InfisicalHttpResponse response = _invoker.InvokeWithCandidateFallback(connection, InfisicalEndpointNames.RetrieveCertificate, "RetrieveCertificate", pathParameters, null, null);
                string body = response.Body;
                response.Clear();

                InfisicalCertificateResponseDto inner = ParseCertificateSingleBody(body);
                InfisicalCertificate mapped = InfisicalCertificateMapper.Map(inner, connection.ProjectId);
                _logger.Information(Component, "Infisical certificate retrieval was successful.");
                return mapped;
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical certificate retrieval failed.");
                throw;
            }
        }

        private List<InfisicalInternalCaResponseDto> ParseCaListBody(string body)
        {
            if (string.IsNullOrEmpty(body)) { return null; }
            JToken token = JToken.Parse(body);
            if (token.Type == JTokenType.Array)
            {
                return token.ToObject<List<InfisicalInternalCaResponseDto>>();
            }

            InfisicalInternalCaListResponseDto wrapper = token.ToObject<InfisicalInternalCaListResponseDto>();
            return wrapper != null ? (wrapper.CertificateAuthorities ?? wrapper.Cas) : null;
        }

        private InfisicalInternalCaResponseDto ParseCaSingleBody(string body)
        {
            if (string.IsNullOrEmpty(body)) { return null; }
            JToken token = JToken.Parse(body);
            if (token.Type != JTokenType.Object) { return null; }
            JObject obj = (JObject)token;

            if (obj["certificateAuthority"] is JObject ca1) { return ca1.ToObject<InfisicalInternalCaResponseDto>(); }
            if (obj["ca"] is JObject ca2) { return ca2.ToObject<InfisicalInternalCaResponseDto>(); }
            return obj.ToObject<InfisicalInternalCaResponseDto>();
        }

        private InfisicalCertificateResponseDto ParseCertificateSingleBody(string body)
        {
            if (string.IsNullOrEmpty(body)) { return null; }
            JToken token = JToken.Parse(body);
            if (token.Type != JTokenType.Object) { return null; }
            JObject obj = (JObject)token;

            if (obj["certificate"] is JObject cert) { return cert.ToObject<InfisicalCertificateResponseDto>(); }
            return obj.ToObject<InfisicalCertificateResponseDto>();
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

        public InfisicalSignedCertificate SignCertificateBySubscriber(InfisicalConnection connection, string subscriberName, string projectId, string csrPem)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            if (string.IsNullOrEmpty(subscriberName)) { throw new InfisicalConfigurationException("SubscriberName is required."); }
            if (string.IsNullOrEmpty(csrPem)) { throw new InfisicalConfigurationException("CSR is required."); }
            string resolvedProjectId = FirstNonEmpty(projectId, connection.ProjectId);
            if (string.IsNullOrEmpty(resolvedProjectId)) { throw new InfisicalConfigurationException("ProjectId is required."); }

            Dictionary<string, string> pathParameters = new Dictionary<string, string> { { "subscriberName", subscriberName } };
            InfisicalSignCertificateBySubscriberRequestDto request = new InfisicalSignCertificateBySubscriberRequestDto
            {
                ProjectId = resolvedProjectId,
                Csr = csrPem
            };
            string body = _serializer.Serialize(request);

            try
            {
                _logger.Information(Component, string.Concat("Attempting to sign certificate via subscriber '", subscriberName, "'. Please Wait..."));
                InfisicalHttpResponse response = _invoker.InvokeWithCandidateFallback(connection, InfisicalEndpointNames.SignCertificateBySubscriber, "SignCertificateBySubscriber", pathParameters, null, body);
                InfisicalSignCertificateResponseDto dto = _serializer.Deserialize<InfisicalSignCertificateResponseDto>(response.Body);
                response.Clear();

                InfisicalSignedCertificate signed = MapSigned(dto);
                _logger.Information(Component, "Infisical certificate signing (subscriber) was successful.");
                return signed;
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical certificate signing (subscriber) failed.");
                throw;
            }
        }

        public InfisicalSignedCertificate SignCertificateByCa(InfisicalConnection connection, string caId, string csrPem, string commonName, string altNames, string ttl, string notBefore, string notAfter, string friendlyName, string pkiCollectionId, IEnumerable<string> keyUsages, IEnumerable<string> extendedKeyUsages)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            if (string.IsNullOrEmpty(caId)) { throw new InfisicalConfigurationException("CaId is required."); }
            if (string.IsNullOrEmpty(csrPem)) { throw new InfisicalConfigurationException("CSR is required."); }
            if (string.IsNullOrEmpty(ttl) && string.IsNullOrEmpty(notAfter)) { throw new InfisicalConfigurationException("Either Ttl or NotAfter must be provided."); }

            Dictionary<string, string> pathParameters = new Dictionary<string, string> { { "caId", caId } };
            InfisicalSignCertificateByCaRequestDto request = new InfisicalSignCertificateByCaRequestDto
            {
                Csr = csrPem,
                CommonName = commonName,
                AltNames = altNames,
                Ttl = ttl,
                NotBefore = notBefore,
                NotAfter = notAfter,
                FriendlyName = friendlyName,
                PkiCollectionId = pkiCollectionId,
                KeyUsages = keyUsages != null ? keyUsages.ToList() : null,
                ExtendedKeyUsages = extendedKeyUsages != null ? extendedKeyUsages.ToList() : null
            };
            string body = _serializer.Serialize(request);

            try
            {
                _logger.Information(Component, string.Concat("Attempting to sign certificate via CA '", caId, "'. Please Wait..."));
                InfisicalHttpResponse response = _invoker.InvokeWithCandidateFallback(connection, InfisicalEndpointNames.SignCertificateByCa, "SignCertificateByCa", pathParameters, null, body);
                InfisicalSignCertificateResponseDto dto = _serializer.Deserialize<InfisicalSignCertificateResponseDto>(response.Body);
                response.Clear();

                InfisicalSignedCertificate signed = MapSigned(dto);
                _logger.Information(Component, "Infisical certificate signing (CA) was successful.");
                return signed;
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical certificate signing (CA) failed.");
                throw;
            }
        }

        private static InfisicalSignedCertificate MapSigned(InfisicalSignCertificateResponseDto dto)
        {
            if (dto == null) { return null; }
            return new InfisicalSignedCertificate
            {
                SerialNumber = dto.SerialNumber,
                CertificatePem = dto.Certificate,
                CertificateChainPem = dto.CertificateChain,
                IssuingCaCertificatePem = dto.IssuingCaCertificate
            };
        }

        public InfisicalPkiSubscriber[] ListPkiSubscribers(InfisicalConnection connection, string projectId)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            string resolvedProjectId = FirstNonEmpty(projectId, connection.ProjectId);
            if (string.IsNullOrEmpty(resolvedProjectId)) { throw new InfisicalConfigurationException("ProjectId is required."); }

            Dictionary<string, string> pathParameters = new Dictionary<string, string> { { "projectId", resolvedProjectId } };

            try
            {
                _logger.Information(Component, "Attempting to list Infisical PKI subscribers. Please Wait...");
                InfisicalHttpResponse response = _invoker.InvokeWithCandidateFallback(connection, InfisicalEndpointNames.ListPkiSubscribers, "ListPkiSubscribers", pathParameters, null, null);
                string body = response.Body;
                response.Clear();

                List<InfisicalPkiSubscriberResponseDto> source = ParsePkiSubscriberListBody(body);
                InfisicalPkiSubscriber[] mapped = InfisicalPkiSubscriberMapper.MapMany(source, resolvedProjectId);
                _logger.Information(Component, "Infisical PKI subscriber list retrieval was successful.");
                return mapped;
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical PKI subscriber list retrieval failed.");
                throw;
            }
        }

        public InfisicalPkiSubscriber GetPkiSubscriber(InfisicalConnection connection, string subscriberName, string projectId)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }
            if (string.IsNullOrEmpty(subscriberName)) { throw new InfisicalConfigurationException("SubscriberName is required."); }
            string resolvedProjectId = FirstNonEmpty(projectId, connection.ProjectId);
            if (string.IsNullOrEmpty(resolvedProjectId)) { throw new InfisicalConfigurationException("ProjectId is required."); }

            Dictionary<string, string> pathParameters = new Dictionary<string, string> { { "subscriberName", subscriberName } };
            List<KeyValuePair<string, string>> query = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("projectId", resolvedProjectId) };

            try
            {
                _logger.Information(Component, string.Concat("Attempting to retrieve Infisical PKI subscriber '", subscriberName, "'. Please Wait..."));
                InfisicalHttpResponse response = _invoker.InvokeWithCandidateFallback(connection, InfisicalEndpointNames.GetPkiSubscriber, "GetPkiSubscriber", pathParameters, query, null);
                InfisicalPkiSubscriberResponseDto dto = _serializer.Deserialize<InfisicalPkiSubscriberResponseDto>(response.Body);
                response.Clear();

                InfisicalPkiSubscriber mapped = InfisicalPkiSubscriberMapper.Map(dto, resolvedProjectId);
                _logger.Information(Component, "Infisical PKI subscriber retrieval was successful.");
                return mapped;
            }
            catch (Exception)
            {
                _logger.Error(Component, "Infisical PKI subscriber retrieval failed.");
                throw;
            }
        }

        private List<InfisicalPkiSubscriberResponseDto> ParsePkiSubscriberListBody(string body)
        {
            if (string.IsNullOrEmpty(body)) { return null; }
            JToken token = JToken.Parse(body);
            if (token.Type == JTokenType.Array)
            {
                return token.ToObject<List<InfisicalPkiSubscriberResponseDto>>();
            }

            InfisicalPkiSubscriberListResponseDto wrapper = token.ToObject<InfisicalPkiSubscriberListResponseDto>();
            return wrapper != null ? wrapper.Subscribers : null;
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
