using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using PSInfisicalAPI.Pki;
using Xunit;

namespace PSInfisicalAPI.Tests
{
    public class PemCertificateBuilderTests
    {
        private static (string CertPem, string KeyPem, string Thumbprint) CreateSelfSigned(string commonName)
        {
            using (RSA rsa = RSA.Create(2048))
            {
                CertificateRequest request = new CertificateRequest(
                    "CN=" + commonName,
                    rsa,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1);

                DateTimeOffset notBefore = DateTimeOffset.UtcNow.AddMinutes(-5);
                DateTimeOffset notAfter = DateTimeOffset.UtcNow.AddDays(1);
                using (X509Certificate2 cert = request.CreateSelfSigned(notBefore, notAfter))
                {
                    byte[] derBytes = cert.Export(X509ContentType.Cert);
                    string certPem = "-----BEGIN CERTIFICATE-----\n" +
                                     Convert.ToBase64String(derBytes, Base64FormattingOptions.InsertLineBreaks) +
                                     "\n-----END CERTIFICATE-----\n";

                    byte[] pkcs8 = rsa.ExportPkcs8PrivateKey();
                    string keyPem = "-----BEGIN PRIVATE KEY-----\n" +
                                    Convert.ToBase64String(pkcs8, Base64FormattingOptions.InsertLineBreaks) +
                                    "\n-----END PRIVATE KEY-----\n";

                    return (certPem, keyPem, cert.Thumbprint);
                }
            }
        }

        [Fact]
        public void Build_From_Cert_Only_Returns_X509Certificate2_Without_Key()
        {
            (string certPem, _, string thumbprint) = CreateSelfSigned("PemBuilderTest.NoKey");
            X509Certificate2 cert = PemCertificateBuilder.Build(certPem, null, null, X509KeyStorageFlags.DefaultKeySet);
            try
            {
                Assert.NotNull(cert);
                Assert.Equal(thumbprint, cert.Thumbprint);
                Assert.False(cert.HasPrivateKey);
            }
            finally
            {
                cert.Dispose();
            }
        }

        [Fact]
        public void Build_With_Pkcs8_Key_Attaches_Private_Key()
        {
            (string certPem, string keyPem, string thumbprint) = CreateSelfSigned("PemBuilderTest.WithKey");
            X509Certificate2 cert = PemCertificateBuilder.Build(certPem, keyPem, null, X509KeyStorageFlags.Exportable);
            try
            {
                Assert.NotNull(cert);
                Assert.Equal(thumbprint, cert.Thumbprint);
                Assert.True(cert.HasPrivateKey);
            }
            finally
            {
                cert.Dispose();
            }
        }

        [Fact]
        public void ReadCertificateChain_Returns_All_Certificates()
        {
            (string leafPem, _, _) = CreateSelfSigned("PemBuilderTest.Leaf");
            (string intermediatePem, _, _) = CreateSelfSigned("PemBuilderTest.Intermediate");
            string combined = leafPem + intermediatePem;

            System.Collections.Generic.List<X509Certificate2> chain = PemCertificateBuilder.ReadCertificateChain(combined);
            try
            {
                Assert.Equal(2, chain.Count);
            }
            finally
            {
                foreach (X509Certificate2 c in chain) { c.Dispose(); }
            }
        }

        [Fact]
        public void Build_Empty_Certificate_Pem_Throws()
        {
            Assert.Throws<ArgumentException>(() => PemCertificateBuilder.Build(null, null, null, X509KeyStorageFlags.DefaultKeySet));
        }
    }
}
