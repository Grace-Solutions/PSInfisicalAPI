using System;
using System.Collections.Generic;
using System.Globalization;
using PSInfisicalAPI.Models;

namespace PSInfisicalAPI.Pki
{
    internal static class InfisicalPkiSubscriberMapper
    {
        public static InfisicalPkiSubscriber Map(InfisicalPkiSubscriberResponseDto dto, string fallbackProjectId)
        {
            if (dto == null)
            {
                return null;
            }

            return new InfisicalPkiSubscriber
            {
                Id = dto.Id,
                ProjectId = !string.IsNullOrEmpty(dto.ProjectId) ? dto.ProjectId : fallbackProjectId,
                CaId = dto.CaId,
                Name = dto.Name,
                CommonName = dto.CommonName,
                Status = dto.Status,
                Ttl = dto.Ttl,
                SubjectAlternativeNames = dto.SubjectAlternativeNames != null ? dto.SubjectAlternativeNames.ToArray() : null,
                KeyUsages = dto.KeyUsages != null ? dto.KeyUsages.ToArray() : null,
                ExtendedKeyUsages = dto.ExtendedKeyUsages != null ? dto.ExtendedKeyUsages.ToArray() : null,
                EnableAutoRenewal = dto.EnableAutoRenewal,
                AutoRenewalPeriodInDays = dto.AutoRenewalPeriodInDays,
                LastOperationStatus = dto.LastOperationStatus,
                LastOperationMessage = dto.LastOperationMessage,
                LastOperationAtUtc = ParseTimestamp(dto.LastOperationAt),
                CreatedAtUtc = ParseTimestamp(dto.CreatedAt),
                UpdatedAtUtc = ParseTimestamp(dto.UpdatedAt),
                Properties = MapProperties(dto.Properties)
            };
        }

        public static InfisicalPkiSubscriber[] MapMany(IEnumerable<InfisicalPkiSubscriberResponseDto> items, string fallbackProjectId)
        {
            if (items == null)
            {
                return Array.Empty<InfisicalPkiSubscriber>();
            }

            List<InfisicalPkiSubscriber> results = new List<InfisicalPkiSubscriber>();
            foreach (InfisicalPkiSubscriberResponseDto dto in items)
            {
                InfisicalPkiSubscriber mapped = Map(dto, fallbackProjectId);
                if (mapped != null)
                {
                    results.Add(mapped);
                }
            }

            return results.ToArray();
        }

        private static InfisicalPkiSubscriberProperties MapProperties(InfisicalPkiSubscriberPropertiesDto dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new InfisicalPkiSubscriberProperties
            {
                AzureTemplateType = dto.AzureTemplateType,
                Organization = dto.Organization,
                OrganizationalUnit = dto.OrganizationalUnit,
                Country = dto.Country,
                State = dto.State,
                Locality = dto.Locality,
                EmailAddress = dto.EmailAddress
            };
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
