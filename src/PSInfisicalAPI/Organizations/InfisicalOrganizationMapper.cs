using System;
using System.Collections.Generic;
using System.Globalization;
using PSInfisicalAPI.Models;

namespace PSInfisicalAPI.Organizations
{
    internal static class InfisicalOrganizationMapper
    {
        public static InfisicalOrganization Map(InfisicalOrganizationResponseDto dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new InfisicalOrganization
            {
                Id = !string.IsNullOrEmpty(dto.Id) ? dto.Id : dto.InternalId,
                Name = dto.Name,
                Slug = dto.Slug,
                CustomerId = dto.CustomerId,
                AuthEnforced = dto.AuthEnforced,
                ScimEnabled = dto.ScimEnabled,
                CreatedAtUtc = ParseTimestamp(dto.CreatedAt),
                UpdatedAtUtc = ParseTimestamp(dto.UpdatedAt)
            };
        }

        public static InfisicalOrganization[] MapMany(IEnumerable<InfisicalOrganizationResponseDto> items)
        {
            if (items == null)
            {
                return Array.Empty<InfisicalOrganization>();
            }

            List<InfisicalOrganization> results = new List<InfisicalOrganization>();
            foreach (InfisicalOrganizationResponseDto dto in items)
            {
                InfisicalOrganization mapped = Map(dto);
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
