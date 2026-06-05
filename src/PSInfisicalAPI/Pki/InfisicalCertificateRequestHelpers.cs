using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using PSInfisicalAPI.Logging;
using PSInfisicalAPI.Models;

namespace PSInfisicalAPI.Pki
{
    internal static class InfisicalCertificateRequestHelpers
    {
        public static InfisicalCsrSubject MergeSubject(IDictionary subject, string commonName, string country, string state, string locality, string organization, string organizationalUnit, string emailAddress)
        {
            InfisicalCsrSubject result = new InfisicalCsrSubject();
            if (subject != null)
            {
                result.CommonName = ReadString(subject, "CN", "CommonName");
                result.Country = ReadString(subject, "C", "Country");
                result.State = ReadString(subject, "ST", "S", "State");
                result.Locality = ReadString(subject, "L", "Locality");
                result.Organization = ReadString(subject, "O", "Organization");
                result.OrganizationalUnit = ReadString(subject, "OU", "OrganizationalUnit");
                result.EmailAddress = ReadString(subject, "E", "EMAIL", "EmailAddress");
            }

            if (!string.IsNullOrEmpty(commonName)) { result.CommonName = commonName; }
            if (!string.IsNullOrEmpty(country)) { result.Country = country; }
            if (!string.IsNullOrEmpty(state)) { result.State = state; }
            if (!string.IsNullOrEmpty(locality)) { result.Locality = locality; }
            if (!string.IsNullOrEmpty(organization)) { result.Organization = organization; }
            if (!string.IsNullOrEmpty(organizationalUnit)) { result.OrganizationalUnit = organizationalUnit; }
            if (!string.IsNullOrEmpty(emailAddress)) { result.EmailAddress = emailAddress; }

            return result;
        }

        public static string ResolveLocalFqdn()
        {
            try
            {
                string host = System.Net.Dns.GetHostName();
                string domain = null;
                try { domain = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName; }
                catch { domain = null; }

                if (!string.IsNullOrEmpty(domain) && !host.EndsWith("." + domain, StringComparison.OrdinalIgnoreCase))
                {
                    return string.Concat(host, ".", domain);
                }

                return host;
            }
            catch
            {
                return null;
            }
        }

        public static void InstallToStore(X509Certificate2 cert, StoreName storeName, StoreLocation storeLocation, bool force, IInfisicalLogger logger, string component)
        {
            X509Store store = new X509Store(storeName, storeLocation);
            try
            {
                store.Open(OpenFlags.ReadWrite);
                X509Certificate2Collection existing = store.Certificates.Find(X509FindType.FindByThumbprint, cert.Thumbprint, false);
                string target = string.Concat(storeLocation.ToString(), @"\", storeName.ToString(), " [", cert.Thumbprint, "]");
                if (existing.Count > 0)
                {
                    if (!force)
                    {
                        logger.Information(component, string.Concat("Certificate already present in ", target, "; no action taken."));
                        return;
                    }

                    store.RemoveRange(existing);
                }

                store.Add(cert);
                logger.Information(component, string.Concat("Installed certificate to ", target, "."));
            }
            finally
            {
                store.Close();
            }
        }

        public static void InstallChain(InfisicalSignedCertificate signed, StoreLocation storeLocation, bool force, IInfisicalLogger logger, string component)
        {
            List<X509Certificate2> chainCerts = CollectChainCertificates(signed);
            InstallChain(chainCerts, storeLocation, force, logger, component);
        }

        public static void InstallChain(IEnumerable<X509Certificate2> chainCerts, StoreLocation storeLocation, bool force, IInfisicalLogger logger, string component)
        {
            if (chainCerts == null) { return; }
            foreach (X509Certificate2 chainCert in chainCerts)
            {
                if (chainCert == null) { continue; }
                StoreName targetStore = GetChainCertificateTargetStore(chainCert);
                InstallToStore(chainCert, targetStore, storeLocation, force, logger, component);
            }
        }

        public static StoreName GetChainCertificateTargetStore(X509Certificate2 cert)
        {
            return IsSelfSigned(cert) ? StoreName.Root : StoreName.CertificateAuthority;
        }

        public static X509KeyStorageFlags ResolveKeyStorageFlags(InfisicalPrivateKeyProtection protection, bool persistKey, bool machineKey)
        {
            X509KeyStorageFlags flags = X509KeyStorageFlags.DefaultKeySet;
            switch (protection)
            {
                case InfisicalPrivateKeyProtection.Exportable:
                    flags |= X509KeyStorageFlags.Exportable;
                    break;
                case InfisicalPrivateKeyProtection.Ephemeral:
                    const int ephemeralValue = 32;
                    if (Enum.GetName(typeof(X509KeyStorageFlags), ephemeralValue) == null)
                    {
                        throw new PlatformNotSupportedException("InfisicalPrivateKeyProtection.Ephemeral requires .NET Core 3.0 or later (PowerShell 7+). Use LocalOnly or NonExportable on Windows PowerShell 5.1.");
                    }
                    flags |= (X509KeyStorageFlags)ephemeralValue;
                    break;
            }

            if (machineKey) { flags |= X509KeyStorageFlags.MachineKeySet; }
            if (persistKey) { flags |= X509KeyStorageFlags.PersistKeySet; }

            return flags;
        }

        public static bool ShouldScrubPrivateKeyPem(InfisicalPrivateKeyProtection protection, bool hasExplicitPrivateKeyPath)
        {
            if (hasExplicitPrivateKeyPath) { return true; }
            return protection == InfisicalPrivateKeyProtection.NonExportable
                || protection == InfisicalPrivateKeyProtection.Ephemeral;
        }

        public static void WritePrivateKeyPem(string privateKeyPem, string path)
        {
            if (string.IsNullOrEmpty(privateKeyPem)) { throw new ArgumentException("PrivateKeyPem is empty.", nameof(privateKeyPem)); }
            if (string.IsNullOrEmpty(path)) { throw new ArgumentException("Path is required.", nameof(path)); }

            string fullPath = System.IO.Path.GetFullPath(path);
            string directory = System.IO.Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            System.IO.File.WriteAllText(fullPath, privateKeyPem);
        }

        public static InfisicalCertificateResult BuildResultFromExistingLocal(X509Certificate2 leaf)
        {
            return BuildResultFromExistingLocal(leaf, null);
        }

        public static InfisicalCertificateResult BuildResultFromExistingLocal(X509Certificate2 leaf, InfisicalCertificateBundle fallbackBundle)
        {
            if (leaf == null) { throw new ArgumentNullException(nameof(leaf)); }

            InfisicalCertificateResult result = new InfisicalCertificateResult
            {
                Leaf = leaf,
                SerialNumber = leaf.SerialNumber,
                CertificatePem = ExportCertificateToPem(leaf)
            };

            List<X509Certificate2> chainElements = BuildLocalChain(leaf);

            if (fallbackBundle != null && !string.IsNullOrEmpty(fallbackBundle.CertificateChainPem))
            {
                List<X509Certificate2> bundleChain = PemCertificateBuilder.ReadCertificateChain(fallbackBundle.CertificateChainPem);
                HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (X509Certificate2 c in chainElements) { if (c != null) { seen.Add(c.Thumbprint); } }
                foreach (X509Certificate2 c in bundleChain)
                {
                    if (c == null) { continue; }
                    if (seen.Add(c.Thumbprint)) { chainElements.Add(c); }
                }
            }

            List<X509Certificate2> intermediates = new List<X509Certificate2>();
            X509Certificate2 root = null;
            foreach (X509Certificate2 cert in chainElements)
            {
                if (string.Equals(cert.Thumbprint, leaf.Thumbprint, StringComparison.OrdinalIgnoreCase)) { continue; }
                if (IsSelfSigned(cert)) { if (root == null) { root = cert; } }
                else { intermediates.Add(cert); }
            }

            result.Intermediates = intermediates.ToArray();
            result.Root = root;

            List<X509Certificate2> ordered = new List<X509Certificate2> { leaf };
            ordered.AddRange(intermediates);
            if (root != null) { ordered.Add(root); }
            result.Chain = ordered.ToArray();

            if (intermediates.Count > 0 || root != null)
            {
                StringBuilder sb = new StringBuilder();
                foreach (X509Certificate2 c in intermediates) { sb.Append(ExportCertificateToPem(c)); }
                if (root != null) { sb.Append(ExportCertificateToPem(root)); }
                result.CertificateChainPem = sb.ToString();
            }

            return result;
        }

        public static InfisicalCertificateResult BuildResult(X509Certificate2 leaf, InfisicalSignedCertificate signed)
        {
            InfisicalCertificateResult result = new InfisicalCertificateResult { Leaf = leaf };
            if (signed != null)
            {
                result.SerialNumber = signed.SerialNumber;
                result.CertificatePem = signed.CertificatePem;
                result.CertificateChainPem = signed.CertificateChainPem;
                result.PrivateKeyPem = signed.PrivateKeyPem;
                result.Status = signed.Status;
                result.StatusMessage = signed.StatusMessage;
                result.CertificateRequestId = signed.CertificateRequestId;
            }

            List<X509Certificate2> chainCerts = signed != null ? CollectChainCertificates(signed) : new List<X509Certificate2>();
            List<X509Certificate2> intermediates = new List<X509Certificate2>();
            X509Certificate2 root = null;
            foreach (X509Certificate2 cert in chainCerts)
            {
                if (IsSelfSigned(cert)) { if (root == null) { root = cert; } }
                else { intermediates.Add(cert); }
            }

            result.Intermediates = intermediates.ToArray();
            result.Root = root;

            List<X509Certificate2> ordered = new List<X509Certificate2>();
            if (leaf != null) { ordered.Add(leaf); }
            ordered.AddRange(intermediates);
            if (root != null) { ordered.Add(root); }
            result.Chain = ordered.ToArray();
            return result;
        }

        private static List<X509Certificate2> CollectChainCertificates(InfisicalSignedCertificate signed)
        {
            List<X509Certificate2> chainCerts = PemCertificateBuilder.ReadCertificateChain(signed.CertificateChainPem);
            if (!string.IsNullOrEmpty(signed.IssuingCaCertificatePem))
            {
                foreach (X509Certificate2 issuing in PemCertificateBuilder.ReadCertificateChain(signed.IssuingCaCertificatePem))
                {
                    chainCerts.Add(issuing);
                }
            }

            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<X509Certificate2> deduped = new List<X509Certificate2>();
            foreach (X509Certificate2 cert in chainCerts)
            {
                if (cert == null) { continue; }
                if (seen.Add(cert.Thumbprint)) { deduped.Add(cert); }
            }

            return deduped;
        }

        private static bool IsSelfSigned(X509Certificate2 cert)
        {
            if (cert == null) { return false; }
            return string.Equals(cert.Subject, cert.Issuer, StringComparison.OrdinalIgnoreCase);
        }

        private static List<X509Certificate2> BuildLocalChain(X509Certificate2 leaf)
        {
            List<X509Certificate2> result = new List<X509Certificate2>();
            using (X509Chain chain = new X509Chain())
            {
                chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                chain.ChainPolicy.VerificationFlags =
                    X509VerificationFlags.IgnoreNotTimeValid |
                    X509VerificationFlags.IgnoreNotTimeNested |
                    X509VerificationFlags.IgnoreInvalidName |
                    X509VerificationFlags.IgnoreInvalidPolicy |
                    X509VerificationFlags.IgnoreEndRevocationUnknown |
                    X509VerificationFlags.IgnoreCertificateAuthorityRevocationUnknown |
                    X509VerificationFlags.IgnoreRootRevocationUnknown |
                    X509VerificationFlags.IgnoreCtlNotTimeValid |
                    X509VerificationFlags.IgnoreCtlSignerRevocationUnknown |
                    X509VerificationFlags.IgnoreInvalidBasicConstraints |
                    X509VerificationFlags.IgnoreWrongUsage;

                try { chain.Build(leaf); }
                catch { return result; }

                foreach (X509ChainElement element in chain.ChainElements)
                {
                    if (element != null && element.Certificate != null)
                    {
                        result.Add(new X509Certificate2(element.Certificate.RawData));
                    }
                }
            }

            return result;
        }

        private static string ExportCertificateToPem(X509Certificate2 cert)
        {
            byte[] der = cert.Export(X509ContentType.Cert);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("-----BEGIN CERTIFICATE-----");
            sb.AppendLine(Convert.ToBase64String(der, Base64FormattingOptions.InsertLineBreaks));
            sb.AppendLine("-----END CERTIFICATE-----");
            return sb.ToString();
        }

        private static string ReadString(IDictionary source, params string[] keys)
        {
            foreach (string key in keys)
            {
                if (source.Contains(key))
                {
                    object value = source[key];
                    if (value != null)
                    {
                        string text = value.ToString();
                        if (!string.IsNullOrEmpty(text))
                        {
                            return text;
                        }
                    }
                }
            }

            return null;
        }
    }
}
