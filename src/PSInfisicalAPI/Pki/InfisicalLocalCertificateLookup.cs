using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace PSInfisicalAPI.Pki
{
    internal static class InfisicalLocalCertificateLookup
    {
        public static X509Certificate2 FindMatch(StoreName storeName, StoreLocation storeLocation, string commonName, IEnumerable<string> candidateSerialNumbers)
        {
            HashSet<string> serialSet = NormalizeSerials(candidateSerialNumbers);
            string subjectFilter = !string.IsNullOrEmpty(commonName) ? string.Concat("CN=", commonName) : null;

            X509Store store = new X509Store(storeName, storeLocation);
            try
            {
                store.Open(OpenFlags.ReadOnly);

                X509Certificate2 bestMatch = null;
                foreach (X509Certificate2 candidate in store.Certificates)
                {
                    if (subjectFilter != null && candidate.Subject.IndexOf(subjectFilter, StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        continue;
                    }

                    if (serialSet.Count > 0)
                    {
                        string normalizedSerial = NormalizeSerial(candidate.SerialNumber);
                        if (!serialSet.Contains(normalizedSerial))
                        {
                            continue;
                        }
                    }

                    if (bestMatch == null || candidate.NotAfter > bestMatch.NotAfter)
                    {
                        bestMatch = candidate;
                    }
                }

                return bestMatch;
            }
            finally
            {
                store.Close();
            }
        }

        public static bool IsRenewable(X509Certificate2 cert, int renewalThresholdDays)
        {
            if (cert == null) { return true; }
            DateTime threshold = DateTime.UtcNow.AddDays(renewalThresholdDays);
            return cert.NotAfter.ToUniversalTime() <= threshold;
        }

        private static HashSet<string> NormalizeSerials(IEnumerable<string> serials)
        {
            HashSet<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (serials == null) { return set; }
            foreach (string serial in serials)
            {
                string normalized = NormalizeSerial(serial);
                if (!string.IsNullOrEmpty(normalized))
                {
                    set.Add(normalized);
                }
            }

            return set;
        }

        private static string NormalizeSerial(string value)
        {
            if (string.IsNullOrEmpty(value)) { return null; }
            string trimmed = value.Trim();
            if (trimmed.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                trimmed = trimmed.Substring(2);
            }

            return trimmed.Replace(":", string.Empty).Replace(" ", string.Empty).TrimStart('0');
        }
    }
}
