using System;
using System.Collections.Generic;
using System.Globalization;
using PSInfisicalAPI.Models;

namespace PSInfisicalAPI.SubOrganizations
{
    internal static class InfisicalSubOrganizationMapper
    {
        public static InfisicalSubOrganization Map(InfisicalSubOrganizationResponseDto dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new InfisicalSubOrganization
            {
                Id = !string.IsNullOrEmpty(dto.Id) ? dto.Id : dto.InternalId,
                Name = dto.Name,
                Slug = dto.Slug,
                OrganizationId = !string.IsNullOrEmpty(dto.OrganizationId) ? dto.OrganizationId : dto.OrgId,
                IsAccessible = dto.IsAccessible,
                CreatedAtUtc = ParseTimestamp(dto.CreatedAt),
                UpdatedAtUtc = ParseTimestamp(dto.UpdatedAt)
            };
        }

        public static InfisicalSubOrganization[] MapMany(IEnumerable<InfisicalSubOrganizationResponseDto> items)
        {
            if (items == null)
            {
                return Array.Empty<InfisicalSubOrganization>();
            }

            List<InfisicalSubOrganization> results = new List<InfisicalSubOrganization>();
            foreach (InfisicalSubOrganizationResponseDto dto in items)
            {
                InfisicalSubOrganization mapped = Map(dto);
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
