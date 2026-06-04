using System;
using System.Collections.Generic;
using System.Globalization;
using PSInfisicalAPI.Models;

namespace PSInfisicalAPI.Projects
{
    internal static class InfisicalProjectMapper
    {
        public static InfisicalProject Map(InfisicalProjectResponseDto dto)
        {
            if (dto == null)
            {
                return null;
            }

            InfisicalProject project = new InfisicalProject
            {
                Id = !string.IsNullOrEmpty(dto.Id) ? dto.Id : dto.InternalId,
                Name = dto.Name,
                Slug = dto.Slug,
                Description = dto.Description,
                OrganizationId = !string.IsNullOrEmpty(dto.Organization) ? dto.Organization : dto.OrgId,
                Type = dto.Type,
                AutoCapitalization = dto.AutoCapitalization,
                EnvironmentSlugs = MapEnvironmentSlugs(dto.Environments),
                CreatedAtUtc = ParseTimestamp(dto.CreatedAt),
                UpdatedAtUtc = ParseTimestamp(dto.UpdatedAt)
            };

            return project;
        }

        public static InfisicalProject[] MapMany(IEnumerable<InfisicalProjectResponseDto> items)
        {
            if (items == null)
            {
                return Array.Empty<InfisicalProject>();
            }

            List<InfisicalProject> results = new List<InfisicalProject>();
            foreach (InfisicalProjectResponseDto dto in items)
            {
                InfisicalProject mapped = Map(dto);
                if (mapped != null)
                {
                    results.Add(mapped);
                }
            }

            return results.ToArray();
        }

        private static string[] MapEnvironmentSlugs(List<InfisicalProjectEnvironmentDto> environments)
        {
            if (environments == null || environments.Count == 0)
            {
                return Array.Empty<string>();
            }

            List<string> slugs = new List<string>(environments.Count);
            foreach (InfisicalProjectEnvironmentDto env in environments)
            {
                if (env != null && !string.IsNullOrEmpty(env.Slug))
                {
                    slugs.Add(env.Slug);
                }
            }

            return slugs.ToArray();
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
