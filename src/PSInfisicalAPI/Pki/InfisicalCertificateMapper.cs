using System;
using System.Collections.Generic;
using System.Globalization;
using PSInfisicalAPI.Models;

namespace PSInfisicalAPI.Pki
{
    internal static class InfisicalCertificateMapper
    {
        public static InfisicalCertificate Map(InfisicalCertificateResponseDto dto, string fallbackProjectId)
        {
            if (dto == null)
            {
                return null;
            }

            return new InfisicalCertificate
            {
                Id = dto.Id,
                ProjectId = !string.IsNullOrEmpty(dto.ProjectId) ? dto.ProjectId : fallbackProjectId,
                CaId = dto.CaId,
                CaName = dto.CaName,
                CaCertId = dto.CaCertId,
                CertificateTemplateId = dto.CertificateTemplateId,
                ProfileId = dto.ProfileId,
                ProfileName = dto.ProfileName,
                ApplicationId = dto.ApplicationId,
                ApplicationName = dto.ApplicationName,
                PkiSubscriberId = dto.PkiSubscriberId,
                Status = dto.Status,
                SerialNumber = dto.SerialNumber,
                FriendlyName = dto.FriendlyName,
                CommonName = dto.CommonName,
                AltNames = dto.AltNames,
                KeyUsages = dto.KeyUsages != null ? dto.KeyUsages.ToArray() : null,
                ExtendedKeyUsages = dto.ExtendedKeyUsages != null ? dto.ExtendedKeyUsages.ToArray() : null,
                KeyAlgorithm = dto.KeyAlgorithm,
                SignatureAlgorithm = dto.SignatureAlgorithm,
                SubjectOrganization = dto.SubjectOrganization,
                SubjectOrganizationalUnit = dto.SubjectOrganizationalUnit,
                SubjectCountry = dto.SubjectCountry,
                SubjectState = dto.SubjectState,
                SubjectLocality = dto.SubjectLocality,
                FingerprintSha256 = dto.FingerprintSha256,
                FingerprintSha1 = dto.FingerprintSha1,
                IsCA = dto.IsCA,
                PathLength = dto.PathLength,
                Source = dto.Source,
                EnrollmentType = dto.EnrollmentType,
                HasPrivateKey = dto.HasPrivateKey,
                RevocationReason = dto.RevocationReason,
                RenewalError = dto.RenewalError,
                RenewBeforeDays = dto.RenewBeforeDays,
                RenewedFromCertificateId = dto.RenewedFromCertificateId,
                RenewedByCertificateId = dto.RenewedByCertificateId,
                NotBeforeUtc = ParseTimestamp(dto.NotBefore),
                NotAfterUtc = ParseTimestamp(dto.NotAfter),
                RevokedAtUtc = ParseTimestamp(dto.RevokedAt),
                CreatedAtUtc = ParseTimestamp(dto.CreatedAt),
                UpdatedAtUtc = ParseTimestamp(dto.UpdatedAt)
            };
        }

        public static InfisicalCertificate[] MapMany(IEnumerable<InfisicalCertificateResponseDto> items, string fallbackProjectId)
        {
            if (items == null)
            {
                return Array.Empty<InfisicalCertificate>();
            }

            List<InfisicalCertificate> results = new List<InfisicalCertificate>();
            foreach (InfisicalCertificateResponseDto dto in items)
            {
                InfisicalCertificate mapped = Map(dto, fallbackProjectId);
                if (mapped != null)
                {
                    results.Add(mapped);
                }
            }

            return results.ToArray();
        }

        public static InfisicalCertificateBundle MapBundle(InfisicalCertificateBundleResponseDto dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new InfisicalCertificateBundle
            {
                SerialNumber = dto.SerialNumber,
                CertificatePem = dto.Certificate,
                CertificateChainPem = dto.CertificateChain,
                PrivateKeyPem = dto.PrivateKey
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
