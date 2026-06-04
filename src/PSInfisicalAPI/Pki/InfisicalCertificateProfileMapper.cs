using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Linq;
using PSInfisicalAPI.Models;

namespace PSInfisicalAPI.Pki
{
    internal static class InfisicalCertificateProfileMapper
    {
        public static InfisicalCertificateProfile Map(InfisicalCertificateProfileResponseDto dto, string fallbackProjectId)
        {
            if (dto == null)
            {
                return null;
            }

            return new InfisicalCertificateProfile
            {
                Id = dto.Id,
                ProjectId = !string.IsNullOrEmpty(dto.ProjectId) ? dto.ProjectId : fallbackProjectId,
                CaId = dto.CaId,
                CertificatePolicyId = dto.CertificatePolicyId,
                Slug = dto.Slug,
                Description = dto.Description,
                EnrollmentType = dto.EnrollmentType,
                IssuerType = dto.IssuerType,
                EstConfigId = dto.EstConfigId,
                ApiConfigId = dto.ApiConfigId,
                AcmeConfigId = dto.AcmeConfigId,
                ScepConfigId = dto.ScepConfigId,
                CreatedAtUtc = ParseTimestamp(dto.CreatedAt),
                UpdatedAtUtc = ParseTimestamp(dto.UpdatedAt),
                Defaults = MapDefaults(dto.Defaults),
                CertificateAuthority = MapCa(dto.CertificateAuthority),
                CertificatePolicy = MapPolicy(dto.CertificatePolicy),
                ApiConfig = MapApiConfig(dto.ApiConfig)
            };
        }

        public static InfisicalCertificateProfile[] MapMany(IEnumerable<InfisicalCertificateProfileResponseDto> items, string fallbackProjectId)
        {
            if (items == null)
            {
                return Array.Empty<InfisicalCertificateProfile>();
            }

            List<InfisicalCertificateProfile> results = new List<InfisicalCertificateProfile>();
            foreach (InfisicalCertificateProfileResponseDto dto in items)
            {
                InfisicalCertificateProfile mapped = Map(dto, fallbackProjectId);
                if (mapped != null)
                {
                    results.Add(mapped);
                }
            }

            return results.ToArray();
        }

        private static InfisicalCertificateProfileDefaults MapDefaults(InfisicalCertificateProfileDefaultsDto dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new InfisicalCertificateProfileDefaults
            {
                TtlDays = dto.TtlDays,
                KeyAlgorithm = dto.KeyAlgorithm,
                SignatureAlgorithm = dto.SignatureAlgorithm,
                KeyUsages = FlattenStringOrStringArray(dto.KeyUsagesRaw),
                ExtendedKeyUsages = FlattenStringOrStringArray(dto.ExtendedKeyUsagesRaw)
            };
        }

        private static InfisicalCertificateAuthoritySummary MapCa(InfisicalCertificateAuthoritySummaryDto dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new InfisicalCertificateAuthoritySummary
            {
                Id = dto.Id,
                Status = dto.Status,
                Name = dto.Name,
                IsExternal = dto.IsExternal,
                ExternalType = dto.ExternalType
            };
        }

        private static InfisicalCertificatePolicySummary MapPolicy(InfisicalCertificatePolicySummaryDto dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new InfisicalCertificatePolicySummary
            {
                Id = dto.Id,
                ProjectId = dto.ProjectId,
                Name = dto.Name
            };
        }

        private static InfisicalCertificateProfileApiConfig MapApiConfig(InfisicalCertificateProfileApiConfigDto dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new InfisicalCertificateProfileApiConfig
            {
                Id = dto.Id,
                AutoRenew = dto.AutoRenew,
                RenewBeforeDays = dto.RenewBeforeDays
            };
        }

        internal static string[] FlattenStringOrStringArray(JToken token)
        {
            if (token == null || token.Type == JTokenType.Null) { return null; }
            if (token.Type == JTokenType.String) { return new[] { (string)token }; }
            if (token.Type == JTokenType.Array)
            {
                List<string> items = new List<string>();
                foreach (JToken child in (JArray)token)
                {
                    if (child != null && child.Type == JTokenType.String) { items.Add((string)child); }
                }

                return items.ToArray();
            }

            return null;
        }

        private static DateTimeOffset? ParseTimestamp(string value)
        {
            if (string.IsNullOrEmpty(value)) { return null; }
            DateTimeOffset parsed;
            if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out parsed))
            {
                return parsed;
            }

            return null;
        }
    }
}
