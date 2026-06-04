using System;
using System.Collections.Generic;
using PSInfisicalAPI.Models;

namespace PSInfisicalAPI.Environments
{
    internal static class InfisicalEnvironmentMapper
    {
        public static InfisicalEnvironment Map(InfisicalEnvironmentResponseDto dto, string fallbackProjectId)
        {
            if (dto == null)
            {
                return null;
            }

            string projectId = !string.IsNullOrEmpty(dto.ProjectId)
                ? dto.ProjectId
                : (!string.IsNullOrEmpty(dto.WorkspaceId) ? dto.WorkspaceId : fallbackProjectId);

            return new InfisicalEnvironment
            {
                Id = !string.IsNullOrEmpty(dto.Id) ? dto.Id : dto.InternalId,
                Name = dto.Name,
                Slug = dto.Slug,
                Position = dto.Position,
                ProjectId = projectId
            };
        }

        public static InfisicalEnvironment[] MapMany(IEnumerable<InfisicalEnvironmentResponseDto> items, string fallbackProjectId)
        {
            if (items == null)
            {
                return Array.Empty<InfisicalEnvironment>();
            }

            List<InfisicalEnvironment> results = new List<InfisicalEnvironment>();
            foreach (InfisicalEnvironmentResponseDto dto in items)
            {
                InfisicalEnvironment mapped = Map(dto, fallbackProjectId);
                if (mapped != null)
                {
                    results.Add(mapped);
                }
            }

            return results.ToArray();
        }
    }
}
