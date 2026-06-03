using System;
using System.Collections.Generic;
using System.Globalization;
using PSInfisicalAPI.Models;

namespace PSInfisicalAPI.Tags
{
    internal static class InfisicalTagMapper
    {
        public static InfisicalTag Map(InfisicalTagResponseDto dto, string fallbackProjectId)
        {
            if (dto == null)
            {
                return null;
            }

            string projectId = !string.IsNullOrEmpty(dto.ProjectId)
                ? dto.ProjectId
                : (!string.IsNullOrEmpty(dto.WorkspaceId) ? dto.WorkspaceId : fallbackProjectId);

            return new InfisicalTag
            {
                Id = !string.IsNullOrEmpty(dto.Id) ? dto.Id : dto.InternalId,
                Slug = dto.Slug,
                Name = dto.Name,
                Color = dto.Color,
                ProjectId = projectId,
                CreatedAtUtc = ParseTimestamp(dto.CreatedAt),
                UpdatedAtUtc = ParseTimestamp(dto.UpdatedAt)
            };
        }

        public static InfisicalTag[] MapMany(IEnumerable<InfisicalTagResponseDto> items, string fallbackProjectId)
        {
            if (items == null)
            {
                return Array.Empty<InfisicalTag>();
            }

            List<InfisicalTag> results = new List<InfisicalTag>();
            foreach (InfisicalTagResponseDto dto in items)
            {
                InfisicalTag mapped = Map(dto, fallbackProjectId);
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
