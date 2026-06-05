using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Linq;
using PSInfisicalAPI.Models;

namespace PSInfisicalAPI.Pki
{
    internal static class InfisicalCertificatePolicyMapper
    {
        public static InfisicalCertificatePolicy Map(InfisicalCertificatePolicyResponseDto dto, string fallbackProjectId)
        {
            if (dto == null)
            {
                return null;
            }

            return new InfisicalCertificatePolicy
            {
                Id = dto.Id,
                ProjectId = !string.IsNullOrEmpty(dto.ProjectId) ? dto.ProjectId : fallbackProjectId,
                Name = dto.Name,
                Description = dto.Description,
                Subject = MapSubject(dto.Subject),
                Sans = MapSans(dto.SansRaw),
                KeyUsages = MapUsages(dto.KeyUsages),
                ExtendedKeyUsages = MapUsages(dto.ExtendedKeyUsages),
                Algorithms = MapAlgorithms(dto.Algorithms),
                Validity = MapValidity(dto.Validity),
                CreatedAtUtc = ParseTimestamp(dto.CreatedAt),
                UpdatedAtUtc = ParseTimestamp(dto.UpdatedAt)
            };
        }

        public static InfisicalCertificatePolicy[] MapMany(IEnumerable<InfisicalCertificatePolicyResponseDto> items, string fallbackProjectId)
        {
            if (items == null)
            {
                return Array.Empty<InfisicalCertificatePolicy>();
            }

            List<InfisicalCertificatePolicy> results = new List<InfisicalCertificatePolicy>();
            foreach (InfisicalCertificatePolicyResponseDto dto in items)
            {
                InfisicalCertificatePolicy mapped = Map(dto, fallbackProjectId);
                if (mapped != null)
                {
                    results.Add(mapped);
                }
            }

            return results.ToArray();
        }

        private static InfisicalCertificatePolicySubject MapSubject(InfisicalCertificatePolicySubjectDto dto)
        {
            if (dto == null) { return null; }
            return new InfisicalCertificatePolicySubject
            {
                Type = dto.Type,
                Allowed = InfisicalCertificateProfileMapper.FlattenStringOrStringArray(dto.AllowedRaw)
            };
        }

        private static InfisicalCertificatePolicySan[] MapSans(JToken token)
        {
            if (token == null || token.Type == JTokenType.Null) { return null; }

            List<InfisicalCertificatePolicySan> results = new List<InfisicalCertificatePolicySan>();
            if (token.Type == JTokenType.Array)
            {
                foreach (JToken child in (JArray)token)
                {
                    InfisicalCertificatePolicySan mapped = MapSanObject(child);
                    if (mapped != null) { results.Add(mapped); }
                }
            }
            else if (token.Type == JTokenType.Object)
            {
                InfisicalCertificatePolicySan mapped = MapSanObject(token);
                if (mapped != null) { results.Add(mapped); }
            }

            return results.Count > 0 ? results.ToArray() : null;
        }

        private static InfisicalCertificatePolicySan MapSanObject(JToken token)
        {
            if (token == null || token.Type != JTokenType.Object) { return null; }
            InfisicalCertificatePolicySanDto dto = token.ToObject<InfisicalCertificatePolicySanDto>();
            if (dto == null) { return null; }
            return new InfisicalCertificatePolicySan
            {
                Type = dto.Type,
                Allowed = InfisicalCertificateProfileMapper.FlattenStringOrStringArray(dto.AllowedRaw),
                Required = InfisicalCertificateProfileMapper.FlattenStringOrStringArray(dto.RequiredRaw)
            };
        }

        private static InfisicalCertificatePolicyUsages MapUsages(InfisicalCertificatePolicyUsagesDto dto)
        {
            if (dto == null) { return null; }
            return new InfisicalCertificatePolicyUsages
            {
                Allowed = InfisicalCertificateProfileMapper.FlattenStringOrStringArray(dto.AllowedRaw),
                Required = InfisicalCertificateProfileMapper.FlattenStringOrStringArray(dto.RequiredRaw)
            };
        }

        private static InfisicalCertificatePolicyAlgorithms MapAlgorithms(InfisicalCertificatePolicyAlgorithmsDto dto)
        {
            if (dto == null) { return null; }
            return new InfisicalCertificatePolicyAlgorithms
            {
                Signature = dto.Signature,
                KeyAlgorithms = InfisicalCertificateProfileMapper.FlattenStringOrStringArray(dto.KeyAlgorithmRaw)
            };
        }

        private static InfisicalCertificatePolicyValidity MapValidity(InfisicalCertificatePolicyValidityDto dto)
        {
            if (dto == null) { return null; }
            return new InfisicalCertificatePolicyValidity { Max = dto.Max };
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
