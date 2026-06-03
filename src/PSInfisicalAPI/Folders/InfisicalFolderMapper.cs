using System;
using System.Collections.Generic;
using System.Globalization;
using PSInfisicalAPI.Models;

namespace PSInfisicalAPI.Folders
{
    internal static class InfisicalFolderMapper
    {
        public static InfisicalFolder Map(InfisicalFolderResponseDto dto, string fallbackProjectId, string fallbackEnvironment)
        {
            if (dto == null)
            {
                return null;
            }

            string projectId = !string.IsNullOrEmpty(dto.ProjectId)
                ? dto.ProjectId
                : (!string.IsNullOrEmpty(dto.WorkspaceId) ? dto.WorkspaceId : fallbackProjectId);

            string environment = !string.IsNullOrEmpty(dto.Environment) ? dto.Environment : fallbackEnvironment;

            return new InfisicalFolder
            {
                Id = !string.IsNullOrEmpty(dto.Id) ? dto.Id : dto.InternalId,
                Name = dto.Name,
                Path = dto.Path,
                ParentId = dto.ParentId,
                Environment = environment,
                ProjectId = projectId,
                CreatedAtUtc = ParseTimestamp(dto.CreatedAt),
                UpdatedAtUtc = ParseTimestamp(dto.UpdatedAt)
            };
        }

        public static InfisicalFolder[] MapMany(IEnumerable<InfisicalFolderResponseDto> items, string fallbackProjectId, string fallbackEnvironment)
        {
            if (items == null)
            {
                return Array.Empty<InfisicalFolder>();
            }

            List<InfisicalFolder> results = new List<InfisicalFolder>();
            foreach (InfisicalFolderResponseDto dto in items)
            {
                InfisicalFolder mapped = Map(dto, fallbackProjectId, fallbackEnvironment);
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
