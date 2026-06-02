using System;
using System.Collections.Generic;
using System.Globalization;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Security;

namespace PSInfisicalAPI.Secrets
{
    internal static class InfisicalSecretMapper
    {
        public static InfisicalSecret Map(InfisicalSecretResponseDto dto)
        {
            if (dto == null)
            {
                return null;
            }

            InfisicalSecret secret = new InfisicalSecret
            {
                Id = dto.Id,
                InternalId = dto.InternalId,
                Workspace = dto.Workspace,
                Environment = dto.Environment,
                Version = dto.Version,
                Type = ParseType(dto.Type),
                SecretName = dto.SecretKey,
                SecretValue = SecureStringUtility.ToReadOnlySecureString(dto.SecretValue),
                SecretValueHidden = dto.SecretValueHidden,
                SecretPath = dto.SecretPath,
                SecretComment = dto.SecretComment,
                CreatedAtUtc = ParseTimestamp(dto.CreatedAt),
                UpdatedAtUtc = ParseTimestamp(dto.UpdatedAt),
                IsRotatedSecret = dto.IsRotatedSecret,
                RotationId = ParseGuid(dto.RotationId),
                Tags = MapTags(dto.Tags),
                SecretMetadata = MapMetadata(dto.SecretMetadata)
            };

            dto.SecretValue = null;

            return secret;
        }

        public static InfisicalSecret[] MapMany(IEnumerable<InfisicalSecretResponseDto> items)
        {
            if (items == null)
            {
                return Array.Empty<InfisicalSecret>();
            }

            List<InfisicalSecret> results = new List<InfisicalSecret>();
            foreach (InfisicalSecretResponseDto dto in items)
            {
                InfisicalSecret mapped = Map(dto);
                if (mapped != null)
                {
                    results.Add(mapped);
                }
            }

            return results.ToArray();
        }

        private static InfisicalSecretType ParseType(string value)
        {
            if (string.Equals(value, "personal", StringComparison.OrdinalIgnoreCase))
            {
                return InfisicalSecretType.Personal;
            }

            return InfisicalSecretType.Shared;
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

        private static Guid? ParseGuid(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            Guid parsed;
            if (Guid.TryParse(value, out parsed))
            {
                return parsed;
            }

            return null;
        }

        private static InfisicalSecretTag[] MapTags(List<InfisicalSecretTagDto> tags)
        {
            if (tags == null || tags.Count == 0)
            {
                return Array.Empty<InfisicalSecretTag>();
            }

            InfisicalSecretTag[] mapped = new InfisicalSecretTag[tags.Count];
            for (int index = 0; index < tags.Count; index++)
            {
                InfisicalSecretTagDto tag = tags[index];
                mapped[index] = new InfisicalSecretTag
                {
                    Id = tag.Id,
                    Slug = tag.Slug,
                    Name = tag.Name,
                    Color = tag.Color
                };
            }

            return mapped;
        }

        private static InfisicalSecretMetadata[] MapMetadata(List<InfisicalSecretMetadataDto> metadata)
        {
            if (metadata == null || metadata.Count == 0)
            {
                return Array.Empty<InfisicalSecretMetadata>();
            }

            InfisicalSecretMetadata[] mapped = new InfisicalSecretMetadata[metadata.Count];
            for (int index = 0; index < metadata.Count; index++)
            {
                InfisicalSecretMetadataDto entry = metadata[index];
                mapped[index] = new InfisicalSecretMetadata
                {
                    Key = entry.Key,
                    Value = entry.Value
                };
            }

            return mapped;
        }
    }
}
