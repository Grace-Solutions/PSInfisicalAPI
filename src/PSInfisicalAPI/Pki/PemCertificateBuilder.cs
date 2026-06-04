using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using BcX509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace PSInfisicalAPI.Pki
{
    public static class PemCertificateBuilder
    {
        public static X509Certificate2 Build(string certificatePem, string privateKeyPem, string chainPem, X509KeyStorageFlags storageFlags)
        {
            if (string.IsNullOrEmpty(certificatePem))
            {
                throw new ArgumentException("Certificate PEM is required.", nameof(certificatePem));
            }

            BcX509Certificate leaf = ReadFirstCertificate(certificatePem);
            List<BcX509Certificate> chain = ReadAllCertificates(chainPem);

            if (string.IsNullOrEmpty(privateKeyPem))
            {
                return new X509Certificate2(leaf.GetEncoded());
            }

            AsymmetricKeyParameter privateKey = ReadPrivateKey(privateKeyPem);

            Pkcs12StoreBuilder builder = new Pkcs12StoreBuilder();
            Pkcs12Store store = builder.Build();

            const string alias = "infisical-cert";
            List<X509CertificateEntry> entries = new List<X509CertificateEntry>();
            entries.Add(new X509CertificateEntry(leaf));
            foreach (BcX509Certificate intermediate in chain)
            {
                entries.Add(new X509CertificateEntry(intermediate));
            }

            store.SetKeyEntry(alias, new AsymmetricKeyEntry(privateKey), entries.ToArray());

            char[] password = GenerateRandomPassword();
            using (MemoryStream ms = new MemoryStream())
            {
                store.Save(ms, password, new SecureRandom());
                byte[] pfxBytes = ms.ToArray();
                return new X509Certificate2(pfxBytes, new string(password), storageFlags);
            }
        }

        public static List<X509Certificate2> ReadCertificateChain(string chainPem)
        {
            List<X509Certificate2> results = new List<X509Certificate2>();
            if (string.IsNullOrEmpty(chainPem))
            {
                return results;
            }

            foreach (BcX509Certificate cert in ReadAllCertificates(chainPem))
            {
                results.Add(new X509Certificate2(cert.GetEncoded()));
            }

            return results;
        }

        private static BcX509Certificate ReadFirstCertificate(string pem)
        {
            using (StringReader reader = new StringReader(pem))
            {
                PemReader pemReader = new PemReader(reader);
                object obj = pemReader.ReadObject();
                while (obj != null)
                {
                    BcX509Certificate cert = obj as BcX509Certificate;
                    if (cert != null)
                    {
                        return cert;
                    }

                    obj = pemReader.ReadObject();
                }
            }

            throw new InvalidOperationException("No certificate found in PEM input.");
        }

        private static List<BcX509Certificate> ReadAllCertificates(string pem)
        {
            List<BcX509Certificate> results = new List<BcX509Certificate>();
            if (string.IsNullOrEmpty(pem))
            {
                return results;
            }

            using (StringReader reader = new StringReader(pem))
            {
                PemReader pemReader = new PemReader(reader);
                object obj = pemReader.ReadObject();
                while (obj != null)
                {
                    BcX509Certificate cert = obj as BcX509Certificate;
                    if (cert != null)
                    {
                        results.Add(cert);
                    }

                    obj = pemReader.ReadObject();
                }
            }

            return results;
        }

        private static AsymmetricKeyParameter ReadPrivateKey(string pem)
        {
            using (StringReader reader = new StringReader(pem))
            {
                PemReader pemReader = new PemReader(reader);
                object obj = pemReader.ReadObject();
                while (obj != null)
                {
                    AsymmetricKeyParameter key = obj as AsymmetricKeyParameter;
                    if (key != null && key.IsPrivate)
                    {
                        return key;
                    }

                    AsymmetricCipherKeyPair pair = obj as AsymmetricCipherKeyPair;
                    if (pair != null)
                    {
                        return pair.Private;
                    }

                    obj = pemReader.ReadObject();
                }
            }

            throw new InvalidOperationException("No private key found in PEM input.");
        }

        private static char[] GenerateRandomPassword()
        {
            SecureRandom random = new SecureRandom();
            byte[] bytes = new byte[24];
            random.NextBytes(bytes);
            return Convert.ToBase64String(bytes).ToCharArray();
        }
    }
}
