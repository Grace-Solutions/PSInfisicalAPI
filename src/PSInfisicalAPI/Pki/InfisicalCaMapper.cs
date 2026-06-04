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

            return new InfisicalCertificateAuthority
            {
                Id = dto.Id,
                ProjectId = !string.IsNullOrEmpty(dto.ProjectId) ? dto.ProjectId : fallbackProjectId,
                Name = dto.Name,
                FriendlyName = dto.FriendlyName,
                Type = dto.Type,
                Status = dto.Status,
                EnableDirectIssuance = dto.EnableDirectIssuance,
                KeyAlgorithm = dto.KeyAlgorithm,
                DistinguishedName = dto.DistinguishedName,
                OrganizationName = dto.OrganizationName,
                OrganizationUnit = dto.OrganizationUnit,
                Country = dto.Country,
                State = dto.State,
                Locality = dto.Locality,
                CommonName = dto.CommonName,
                MaxPathLength = dto.MaxPathLength,
                NotBefore = dto.NotBefore,
                NotAfter = dto.NotAfter,
                SerialNumber = dto.SerialNumber,
                ParentCaId = dto.ParentCaId,
                ActiveCaCertId = dto.ActiveCaCertId,
                CreatedAtUtc = ParseTimestamp(dto.CreatedAt),
                UpdatedAtUtc = ParseTimestamp(dto.UpdatedAt)
            };
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
