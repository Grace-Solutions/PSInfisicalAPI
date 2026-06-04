using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using PSInfisicalAPI.Models;

namespace PSInfisicalAPI.Pki
{
    internal static class InfisicalCertificateApplicationMapper
    {
        public static InfisicalCertificateApplication Map(InfisicalCertificateApplicationResponseDto dto, string fallbackProjectId)
        {
            if (dto == null) { return null; }
            return new InfisicalCertificateApplication
            {
                Id = dto.Id,
                ProjectId = !string.IsNullOrEmpty(dto.ProjectId) ? dto.ProjectId : fallbackProjectId,
                Name = dto.Name,
                Description = dto.Description,
                ProfileCount = dto.ProfileCount,
                MemberCount = dto.MemberCount,
                CertificateCount = dto.CertificateCount,
                CreatedAtUtc = ParseTimestamp(dto.CreatedAt),
                UpdatedAtUtc = ParseTimestamp(dto.UpdatedAt)
            };
        }

        public static InfisicalCertificateApplication[] MapMany(IEnumerable<InfisicalCertificateApplicationResponseDto> items, string fallbackProjectId)
        {
            if (items == null) { return Array.Empty<InfisicalCertificateApplication>(); }
            List<InfisicalCertificateApplication> results = new List<InfisicalCertificateApplication>();
            foreach (InfisicalCertificateApplicationResponseDto dto in items)
            {
                InfisicalCertificateApplication mapped = Map(dto, fallbackProjectId);
                if (mapped != null) { results.Add(mapped); }
            }

            return results.ToArray();
        }

        public static InfisicalCertificateApplicationProfileAttachment MapAttachment(InfisicalCertificateApplicationProfileAttachmentDto dto)
        {
            if (dto == null) { return null; }
            return new InfisicalCertificateApplicationProfileAttachment
            {
                ApplicationId = dto.ApplicationId,
                ProfileId = dto.ProfileId,
                ProfileSlug = dto.ProfileSlug,
                ProfileDescription = dto.ProfileDescription,
                ApiConfigId = dto.ApiConfigId,
                EstConfigId = dto.EstConfigId,
                AcmeConfigId = dto.AcmeConfigId,
                ScepConfigId = dto.ScepConfigId,
                CreatedAtUtc = ParseTimestamp(dto.CreatedAt),
                UpdatedAtUtc = ParseTimestamp(dto.UpdatedAt)
            };
        }

        public static InfisicalCertificateApplicationProfileAttachment[] MapAttachments(IEnumerable<InfisicalCertificateApplicationProfileAttachmentDto> items)
        {
            if (items == null) { return Array.Empty<InfisicalCertificateApplicationProfileAttachment>(); }
            List<InfisicalCertificateApplicationProfileAttachment> results = new List<InfisicalCertificateApplicationProfileAttachment>();
            foreach (InfisicalCertificateApplicationProfileAttachmentDto dto in items)
            {
                InfisicalCertificateApplicationProfileAttachment mapped = MapAttachment(dto);
                if (mapped != null) { results.Add(mapped); }
            }

            return results.ToArray();
        }

        public static InfisicalCertificateApplicationEnrollment MapEnrollment(InfisicalCertificateApplicationEnrollmentResponseDto dto)
        {
            if (dto == null) { return null; }
            return new InfisicalCertificateApplicationEnrollment
            {
                ApplicationId = dto.ApplicationId,
                ProfileId = dto.ProfileId,
                Api = MapApi(dto.Api),
                Est = MapEst(dto.Est),
                Acme = MapAcme(dto.Acme),
                Scep = MapScep(dto.Scep),
                EstConfigured = dto.EstConfigured.GetValueOrDefault(),
                AcmeConfigured = dto.AcmeConfigured.GetValueOrDefault(),
                ScepConfigured = dto.ScepConfigured.GetValueOrDefault()
            };
        }

        private static InfisicalCertificateApplicationApiEnrollment MapApi(InfisicalCertificateApplicationApiEnrollmentDto dto)
        {
            if (dto == null) { return null; }
            return new InfisicalCertificateApplicationApiEnrollment { Id = dto.Id, AutoRenew = dto.AutoRenew, RenewBeforeDays = dto.RenewBeforeDays };
        }

        private static InfisicalCertificateApplicationEstEnrollment MapEst(InfisicalCertificateApplicationEstEnrollmentDto dto)
        {
            if (dto == null) { return null; }
            return new InfisicalCertificateApplicationEstEnrollment { Id = dto.Id, DisableBootstrapCaValidation = dto.DisableBootstrapCaValidation, EstEndpointUrl = dto.EstEndpointUrl };
        }

        private static InfisicalCertificateApplicationAcmeEnrollment MapAcme(InfisicalCertificateApplicationAcmeEnrollmentDto dto)
        {
            if (dto == null) { return null; }
            return new InfisicalCertificateApplicationAcmeEnrollment { Id = dto.Id, SkipDnsOwnershipVerification = dto.SkipDnsOwnershipVerification, SkipEabBinding = dto.SkipEabBinding, DirectoryUrl = dto.DirectoryUrl };
        }

        private static InfisicalCertificateApplicationScepEnrollment MapScep(InfisicalCertificateApplicationScepEnrollmentDto dto)
        {
            if (dto == null) { return null; }
            return new InfisicalCertificateApplicationScepEnrollment
            {
                Id = dto.Id,
                ChallengeType = dto.ChallengeType,
                IncludeCaCertInResponse = dto.IncludeCaCertInResponse,
                AllowCertBasedRenewal = dto.AllowCertBasedRenewal,
                DynamicChallengeExpiryMinutes = dto.DynamicChallengeExpiryMinutes,
                DynamicChallengeMaxPending = dto.DynamicChallengeMaxPending,
                ScepEndpointUrl = dto.ScepEndpointUrl,
                ChallengeEndpointUrl = dto.ChallengeEndpointUrl,
                RaCertificatePem = dto.RaCertificatePem,
                RaCertificateThumbprint = ComputeThumbprint(dto.RaCertificatePem),
                RaCertExpiresAtUtc = ParseTimestamp(dto.RaCertExpiresAt)
            };
        }

        internal static string ComputeThumbprint(string pem)
        {
            if (string.IsNullOrEmpty(pem)) { return null; }
            try
            {
                byte[] der = Convert.FromBase64String(StripPemArmor(pem));
                using (X509Certificate2 cert = new X509Certificate2(der))
                {
                    return cert.Thumbprint;
                }
            }
            catch
            {
                return null;
            }
        }

        private static string StripPemArmor(string pem)
        {
            StringBuilder sb = new StringBuilder(pem.Length);
            using (StringReader reader = new StringReader(pem))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string trimmed = line.Trim();
                    if (trimmed.Length == 0) { continue; }
                    if (trimmed.StartsWith("-----", StringComparison.Ordinal)) { continue; }
                    sb.Append(trimmed);
                }
            }

            return sb.ToString();
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
