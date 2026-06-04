using System;
using System.Collections.Generic;
using System.Globalization;
using PSInfisicalAPI.Models;

namespace PSInfisicalAPI.Pki
{
    internal static class InfisicalCaMapper
    {
        public static InfisicalCertificateAuthority Map(InfisicalInternalCaResponseDto dto, string fallbackProjectId)
        {
            if (dto == null)
            {
                return null;
            }

            InfisicalInternalCaConfigurationDto cfg = dto.Configuration;

            return new InfisicalCertificateAuthority
            {
                Id = dto.Id,
                ProjectId = !string.IsNullOrEmpty(dto.ProjectId) ? dto.ProjectId : fallbackProjectId,
                Name = dto.Name,
                FriendlyName = Coalesce(cfg != null ? cfg.FriendlyName : null, dto.FriendlyName),
                Type = Coalesce(dto.Type, cfg != null ? cfg.Type : null),
                Status = dto.Status,
                EnableDirectIssuance = dto.EnableDirectIssuance,
                KeyAlgorithm = Coalesce(cfg != null ? cfg.KeyAlgorithm : null, dto.KeyAlgorithm),
                DistinguishedName = Coalesce(cfg != null ? cfg.DistinguishedName : null, dto.DistinguishedName),
                OrganizationName = Coalesce(cfg != null ? cfg.OrganizationName : null, dto.OrganizationName),
                OrganizationUnit = Coalesce(cfg != null ? cfg.OrganizationUnit : null, dto.OrganizationUnit),
                Country = Coalesce(cfg != null ? cfg.Country : null, dto.Country),
                State = Coalesce(cfg != null ? cfg.State : null, dto.State),
                Locality = Coalesce(cfg != null ? cfg.Locality : null, dto.Locality),
                CommonName = Coalesce(cfg != null ? cfg.CommonName : null, dto.CommonName),
                MaxPathLength = (cfg != null && cfg.MaxPathLength.HasValue) ? cfg.MaxPathLength : dto.MaxPathLength,
                NotBefore = Coalesce(cfg != null ? cfg.NotBefore : null, dto.NotBefore),
                NotAfter = Coalesce(cfg != null ? cfg.NotAfter : null, dto.NotAfter),
                SerialNumber = Coalesce(cfg != null ? cfg.SerialNumber : null, dto.SerialNumber),
                ParentCaId = Coalesce(cfg != null ? cfg.ParentCaId : null, dto.ParentCaId),
                ActiveCaCertId = Coalesce(cfg != null ? cfg.ActiveCaCertId : null, dto.ActiveCaCertId),
                CreatedAtUtc = ParseTimestamp(dto.CreatedAt),
                UpdatedAtUtc = ParseTimestamp(dto.UpdatedAt)
            };
        }

        private static string Coalesce(string primary, string fallback)
        {
            return !string.IsNullOrEmpty(primary) ? primary : fallback;
        }

        public static InfisicalCertificateAuthority[] MapMany(IEnumerable<InfisicalInternalCaResponseDto> items, string fallbackProjectId)
        {
            if (items == null)
            {
                return Array.Empty<InfisicalCertificateAuthority>();
            }

            List<InfisicalCertificateAuthority> results = new List<InfisicalCertificateAuthority>();
            foreach (InfisicalInternalCaResponseDto dto in items)
            {
                InfisicalCertificateAuthority mapped = Map(dto, fallbackProjectId);
                if (mapped != null)
                {
                    results.Add(mapped);
                }
            }

            return results.ToArray();
        }

        private static DateTimeOffset? ParseTimestamp(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            DateTimeOffset parsed;
            if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out parsed))
            {
                return parsed;
            }

            return null;
        }
    }
}
