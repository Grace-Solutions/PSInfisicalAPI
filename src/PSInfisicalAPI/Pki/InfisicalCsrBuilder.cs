using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using BcAttribute = Org.BouncyCastle.Asn1.Cms.Attribute;

namespace PSInfisicalAPI.Pki
{
    public enum InfisicalKeyAlgorithm
    {
        Rsa = 0,
        Ecdsa = 1,
        Ed25519 = 2
    }

    public enum InfisicalEcCurve
    {
        P256 = 0,
        P384 = 1
    }

    public sealed class InfisicalCsrSubject
    {
        public string CommonName { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string Locality { get; set; }
        public string Organization { get; set; }
        public string OrganizationalUnit { get; set; }
        public string EmailAddress { get; set; }
    }

    public sealed class InfisicalCsrOptions
    {
        public InfisicalKeyAlgorithm KeyAlgorithm { get; set; } = InfisicalKeyAlgorithm.Rsa;
        public int RsaKeySize { get; set; } = 2048;
        public InfisicalEcCurve EcCurve { get; set; } = InfisicalEcCurve.P256;
    }

    public sealed class InfisicalCsrResult
    {
        public string CsrPem { get; set; }
        public string PrivateKeyPem { get; set; }
    }

    public static class InfisicalCsrBuilder
    {
        public static InfisicalCsrResult Build(InfisicalCsrSubject subject, IEnumerable<string> dnsNames, IEnumerable<string> ipAddresses, InfisicalCsrOptions options)
        {
            if (subject == null) { throw new ArgumentNullException(nameof(subject)); }
            if (string.IsNullOrEmpty(subject.CommonName)) { throw new ArgumentException("Subject.CommonName is required.", nameof(subject)); }
            if (options == null) { options = new InfisicalCsrOptions(); }

            SecureRandom random = new SecureRandom();
            AsymmetricCipherKeyPair keyPair = GenerateKeyPair(options, random);
            string signatureAlgorithm = ResolveSignatureAlgorithm(options);

            X509Name x509Name = BuildX509Name(subject);
            Asn1Set attributes = BuildSanAttributes(dnsNames, ipAddresses);

            Pkcs10CertificationRequest pkcs10 = new Pkcs10CertificationRequest(signatureAlgorithm, x509Name, keyPair.Public, attributes, keyPair.Private);

            return new InfisicalCsrResult
            {
                CsrPem = WritePem(pkcs10),
                PrivateKeyPem = WritePem(keyPair.Private)
            };
        }

        private static AsymmetricCipherKeyPair GenerateKeyPair(InfisicalCsrOptions options, SecureRandom random)
        {
            switch (options.KeyAlgorithm)
            {
                case InfisicalKeyAlgorithm.Rsa:
                {
                    int keySize = options.RsaKeySize;
                    if (keySize != 2048 && keySize != 3072 && keySize != 4096)
                    {
                        throw new ArgumentException("RsaKeySize must be 2048, 3072, or 4096.", nameof(options));
                    }

                    RsaKeyPairGenerator generator = new RsaKeyPairGenerator();
                    generator.Init(new KeyGenerationParameters(random, keySize));
                    return generator.GenerateKeyPair();
                }
                case InfisicalKeyAlgorithm.Ecdsa:
                {
                    DerObjectIdentifier curveOid = options.EcCurve == InfisicalEcCurve.P384
                        ? SecObjectIdentifiers.SecP384r1
                        : SecObjectIdentifiers.SecP256r1;
                    ECKeyPairGenerator generator = new ECKeyPairGenerator("ECDSA");
                    generator.Init(new ECKeyGenerationParameters(curveOid, random));
                    return generator.GenerateKeyPair();
                }
                case InfisicalKeyAlgorithm.Ed25519:
                {
                    Ed25519KeyPairGenerator generator = new Ed25519KeyPairGenerator();
                    generator.Init(new Ed25519KeyGenerationParameters(random));
                    return generator.GenerateKeyPair();
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(options), options.KeyAlgorithm, "Unsupported KeyAlgorithm.");
            }
        }

        private static string ResolveSignatureAlgorithm(InfisicalCsrOptions options)
        {
            switch (options.KeyAlgorithm)
            {
                case InfisicalKeyAlgorithm.Rsa:
                    return "SHA256WITHRSA";
                case InfisicalKeyAlgorithm.Ecdsa:
                    return options.EcCurve == InfisicalEcCurve.P384 ? "SHA384WITHECDSA" : "SHA256WITHECDSA";
                case InfisicalKeyAlgorithm.Ed25519:
                    return "Ed25519";
                default:
                    throw new ArgumentOutOfRangeException(nameof(options), options.KeyAlgorithm, "Unsupported KeyAlgorithm.");
            }
        }

        private static X509Name BuildX509Name(InfisicalCsrSubject subject)
        {
            List<DerObjectIdentifier> order = new List<DerObjectIdentifier>();
            Dictionary<DerObjectIdentifier, string> values = new Dictionary<DerObjectIdentifier, string>();

            AppendComponent(order, values, X509Name.C, subject.Country);
            AppendComponent(order, values, X509Name.ST, subject.State);
            AppendComponent(order, values, X509Name.L, subject.Locality);
            AppendComponent(order, values, X509Name.O, subject.Organization);
            AppendComponent(order, values, X509Name.OU, subject.OrganizationalUnit);
            AppendComponent(order, values, X509Name.CN, subject.CommonName);
            AppendComponent(order, values, X509Name.EmailAddress, subject.EmailAddress);

            return new X509Name(order, values);
        }

        private static void AppendComponent(List<DerObjectIdentifier> order, Dictionary<DerObjectIdentifier, string> values, DerObjectIdentifier oid, string value)
        {
            if (string.IsNullOrEmpty(value)) { return; }
            order.Add(oid);
            values[oid] = value;
        }

        private static Asn1Set BuildSanAttributes(IEnumerable<string> dnsNames, IEnumerable<string> ipAddresses)
        {
            List<GeneralName> generalNames = new List<GeneralName>();
            if (dnsNames != null)
            {
                foreach (string dns in dnsNames)
                {
                    if (string.IsNullOrEmpty(dns)) { continue; }
                    generalNames.Add(new GeneralName(GeneralName.DnsName, dns));
                }
            }

            if (ipAddresses != null)
            {
                foreach (string ip in ipAddresses)
                {
                    if (string.IsNullOrEmpty(ip)) { continue; }
                    IPAddress parsed;
                    if (!IPAddress.TryParse(ip, out parsed)) { continue; }
                    generalNames.Add(new GeneralName(GeneralName.IPAddress, ip));
                }
            }

            if (generalNames.Count == 0) { return null; }

            GeneralNames sanValue = new GeneralNames(generalNames.ToArray());
            X509Extensions extensions = new X509Extensions(
                new Dictionary<DerObjectIdentifier, X509Extension>
                {
                    { X509Extensions.SubjectAlternativeName, new X509Extension(false, new DerOctetString(sanValue)) }
                });

            BcAttribute extensionRequest = new BcAttribute(PkcsObjectIdentifiers.Pkcs9AtExtensionRequest, new DerSet(extensions));
            return new DerSet(extensionRequest);
        }

        private static string WritePem(object obj)
        {
            using (StringWriter sw = new StringWriter())
            {
                PemWriter pemWriter = new PemWriter(sw);
                pemWriter.WriteObject(obj);
                pemWriter.Writer.Flush();
                return sw.ToString();
            }
        }
    }
}
