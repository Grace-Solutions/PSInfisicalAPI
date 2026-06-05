using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using System.Reflection;
using PSInfisicalAPI.Endpoints;
using PSInfisicalAPI.Pki;
using Xunit;

namespace PSInfisicalAPI.Tests
{
    public class CsrAndRequestCmdletTests
    {
        private static readonly Assembly ModuleAssembly = typeof(PSInfisicalAPI.Connections.InfisicalConnection).Assembly;

        [Fact]
        public void CsrBuilder_Rsa2048_Produces_Pem_Csr_And_PrivateKey_With_Subject_And_Sans()
        {
            InfisicalCsrSubject subject = new InfisicalCsrSubject
            {
                CommonName = "test.contoso.local",
                Organization = "Contoso",
                Country = "US"
            };

            InfisicalCsrOptions options = new InfisicalCsrOptions { KeyAlgorithm = InfisicalKeyAlgorithm.Rsa, RsaKeySize = 2048 };
            InfisicalCsrResult result = InfisicalCsrBuilder.Build(subject, new[] { "test.contoso.local", "alt.contoso.local" }, new[] { "10.0.0.5" }, options);

            Assert.NotNull(result);
            Assert.Contains("BEGIN CERTIFICATE REQUEST", result.CsrPem);
            Assert.Contains("END CERTIFICATE REQUEST", result.CsrPem);
            Assert.Contains("BEGIN RSA PRIVATE KEY", result.PrivateKeyPem);

            Org.BouncyCastle.Pkcs.Pkcs10CertificationRequest pkcs10 = ReadCsr(result.CsrPem);
            Assert.True(pkcs10.Verify());
            Org.BouncyCastle.Crypto.Parameters.RsaKeyParameters rsa = Assert.IsAssignableFrom<Org.BouncyCastle.Crypto.Parameters.RsaKeyParameters>(pkcs10.GetPublicKey());
            Assert.Equal(2048, rsa.Modulus.BitLength);
        }

        [Theory]
        [InlineData(InfisicalEcCurve.P256, "1.2.840.10045.3.1.7")]
        [InlineData(InfisicalEcCurve.P384, "1.3.132.0.34")]
        public void CsrBuilder_Ecdsa_Produces_Verifiable_Csr(InfisicalEcCurve curve, string expectedCurveOid)
        {
            InfisicalCsrSubject subject = new InfisicalCsrSubject { CommonName = "ec.contoso.local" };
            InfisicalCsrOptions options = new InfisicalCsrOptions { KeyAlgorithm = InfisicalKeyAlgorithm.Ecdsa, EcCurve = curve };
            InfisicalCsrResult result = InfisicalCsrBuilder.Build(subject, new[] { "ec.contoso.local" }, null, options);

            Assert.Contains("BEGIN CERTIFICATE REQUEST", result.CsrPem);
            Assert.True(result.PrivateKeyPem.Contains("BEGIN EC PRIVATE KEY") || result.PrivateKeyPem.Contains("BEGIN PRIVATE KEY"));

            Org.BouncyCastle.Pkcs.Pkcs10CertificationRequest pkcs10 = ReadCsr(result.CsrPem);
            Assert.True(pkcs10.Verify());
            Org.BouncyCastle.Crypto.Parameters.ECPublicKeyParameters ec = Assert.IsAssignableFrom<Org.BouncyCastle.Crypto.Parameters.ECPublicKeyParameters>(pkcs10.GetPublicKey());
            Assert.Equal(expectedCurveOid, ec.PublicKeyParamSet.Id);
        }

        [Fact]
        public void CsrBuilder_Ed25519_Produces_Verifiable_Csr()
        {
            InfisicalCsrSubject subject = new InfisicalCsrSubject { CommonName = "ed.contoso.local" };
            InfisicalCsrOptions options = new InfisicalCsrOptions { KeyAlgorithm = InfisicalKeyAlgorithm.Ed25519 };
            InfisicalCsrResult result = InfisicalCsrBuilder.Build(subject, new[] { "ed.contoso.local" }, null, options);

            Assert.Contains("BEGIN CERTIFICATE REQUEST", result.CsrPem);
            Assert.Contains("BEGIN PRIVATE KEY", result.PrivateKeyPem);

            Org.BouncyCastle.Pkcs.Pkcs10CertificationRequest pkcs10 = ReadCsr(result.CsrPem);
            Assert.True(pkcs10.Verify());
            Assert.IsAssignableFrom<Org.BouncyCastle.Crypto.Parameters.Ed25519PublicKeyParameters>(pkcs10.GetPublicKey());
        }

        [Fact]
        public void CsrBuilder_Rsa_Rejects_Invalid_KeySize()
        {
            InfisicalCsrSubject subject = new InfisicalCsrSubject { CommonName = "test.local" };
            InfisicalCsrOptions options = new InfisicalCsrOptions { KeyAlgorithm = InfisicalKeyAlgorithm.Rsa, RsaKeySize = 1024 };
            Assert.Throws<ArgumentException>(() => InfisicalCsrBuilder.Build(subject, null, null, options));
        }

        [Fact]
        public void CsrBuilder_Throws_When_CommonName_Missing()
        {
            InfisicalCsrSubject subject = new InfisicalCsrSubject { Organization = "Contoso" };
            Assert.Throws<ArgumentException>(() => InfisicalCsrBuilder.Build(subject, null, null, new InfisicalCsrOptions()));
        }

        private static Org.BouncyCastle.Pkcs.Pkcs10CertificationRequest ReadCsr(string pem)
        {
            using (System.IO.StringReader reader = new System.IO.StringReader(pem))
            {
                Org.BouncyCastle.OpenSsl.PemReader pemReader = new Org.BouncyCastle.OpenSsl.PemReader(reader);
                object obj = pemReader.ReadObject();
                return Assert.IsType<Org.BouncyCastle.Pkcs.Pkcs10CertificationRequest>(obj);
            }
        }

        [Fact]
        public void MergeSubject_Hashtable_Then_Individual_Params_Override()
        {
            Type helperType = ModuleAssembly.GetType("PSInfisicalAPI.Pki.InfisicalCertificateRequestHelpers", true);
            MethodInfo merge = helperType.GetMethod("MergeSubject", BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(merge);

            Hashtable subject = new Hashtable { { "CN", "fallback.local" }, { "O", "FallbackOrg" }, { "C", "DE" } };
            object result = merge.Invoke(null, new object[] { subject, "explicit.local", null, null, null, "ExplicitOrg", null, null });

            PropertyInfo commonNameProp = result.GetType().GetProperty("CommonName");
            PropertyInfo organizationProp = result.GetType().GetProperty("Organization");
            PropertyInfo countryProp = result.GetType().GetProperty("Country");

            Assert.Equal("explicit.local", commonNameProp.GetValue(result));
            Assert.Equal("ExplicitOrg", organizationProp.GetValue(result));
            Assert.Equal("DE", countryProp.GetValue(result));
        }

        [Fact]
        public void SignCertificateBySubscriber_Uses_Pki_Subscribers_Template()
        {
            IReadOnlyList<InfisicalEndpointDefinition> candidates = InfisicalEndpointRegistry.GetCandidates(InfisicalEndpointNames.SignCertificateBySubscriber);
            Assert.Single(candidates);
            InfisicalEndpointDefinition only = candidates[0];
            Assert.Equal("/api/v1/pki/subscribers/{subscriberName}/sign-certificate", only.Template);
            Assert.Equal("POST", only.Method);
            Assert.True(only.RequiresAuthorization);
            Assert.True(only.ContainsSecretMaterialInResponse);
        }

        [Fact]
        public void Candidates_For_SignCertificateByCa_Include_Pki_And_CertManager()
        {
            IReadOnlyList<InfisicalEndpointDefinition> candidates = InfisicalEndpointRegistry.GetCandidates(InfisicalEndpointNames.SignCertificateByCa);
            Assert.Contains(candidates, c => c.Template == "/api/v1/pki/ca/{caId}/sign-certificate");
            Assert.Contains(candidates, c => c.Template == "/api/v1/cert-manager/ca/{caId}/sign-certificate");
        }

        [Fact]
        public void RequestInfisicalCertificate_Cmdlet_Has_Both_Parameter_Sets()
        {
            Type cmdletType = ModuleAssembly.GetType("PSInfisicalAPI.Cmdlets.RequestInfisicalCertificateCmdlet", true);
            Assert.True(typeof(PSInfisicalAPI.Cmdlets.InfisicalCmdletBase).IsAssignableFrom(cmdletType));

            CustomAttributeData cmdletData = null;
            foreach (CustomAttributeData candidate in cmdletType.GetCustomAttributesData())
            {
                if (candidate.AttributeType == typeof(CmdletAttribute)) { cmdletData = candidate; break; }
            }
            Assert.NotNull(cmdletData);
            Assert.Equal(VerbsLifecycle.Request, cmdletData.ConstructorArguments[0].Value);
            Assert.Equal("InfisicalCertificate", cmdletData.ConstructorArguments[1].Value);

            string defaultParameterSetName = null;
            foreach (CustomAttributeNamedArgument named in cmdletData.NamedArguments)
            {
                if (named.MemberName == "DefaultParameterSetName") { defaultParameterSetName = (string)named.TypedValue.Value; break; }
            }
            Assert.Equal("BySubscriber", defaultParameterSetName);

            Assert.NotNull(cmdletType.GetProperty("PkiSubscriberSlug"));
            Assert.NotNull(cmdletType.GetProperty("CertificateAuthorityId"));
            Assert.NotNull(cmdletType.GetProperty("CertificateProfileId"));
            Assert.NotNull(cmdletType.GetProperty("Subject"));
            Assert.NotNull(cmdletType.GetProperty("CommonName"));
            Assert.NotNull(cmdletType.GetProperty("DnsName"));
            Assert.NotNull(cmdletType.GetProperty("IpAddress"));
            Assert.NotNull(cmdletType.GetProperty("Install"));
            Assert.NotNull(cmdletType.GetProperty("StoreName"));
            Assert.NotNull(cmdletType.GetProperty("StoreLocation"));
            Assert.NotNull(cmdletType.GetProperty("AllowRenewal"));
            Assert.NotNull(cmdletType.GetProperty("RenewalThresholdDays"));
            Assert.NotNull(cmdletType.GetProperty("Force"));
            Assert.NotNull(cmdletType.GetProperty("InstallChain"));

            PropertyInfo keyAlgorithmProp = cmdletType.GetProperty("KeyAlgorithm");
            PropertyInfo curveProp = cmdletType.GetProperty("Curve");
            Assert.NotNull(keyAlgorithmProp);
            Assert.NotNull(curveProp);
            Assert.Equal(typeof(InfisicalKeyAlgorithm), keyAlgorithmProp.PropertyType);
            Assert.Equal(typeof(InfisicalEcCurve), curveProp.PropertyType);

            PropertyInfo protectionProp = cmdletType.GetProperty("PrivateKeyProtection");
            Assert.NotNull(protectionProp);
            Assert.Equal(typeof(InfisicalPrivateKeyProtection), protectionProp.PropertyType);
            Assert.NotNull(cmdletType.GetProperty("PersistKey"));
            Assert.NotNull(cmdletType.GetProperty("MachineKey"));
            Assert.NotNull(cmdletType.GetProperty("PrivateKeyPath"));
            Assert.NotNull(cmdletType.GetProperty("LocalChainOnly"));

            CustomAttributeData outputTypeData = null;
            foreach (CustomAttributeData candidate in cmdletType.GetCustomAttributesData())
            {
                if (candidate.AttributeType == typeof(OutputTypeAttribute)) { outputTypeData = candidate; break; }
            }
            Assert.NotNull(outputTypeData);
            IList<CustomAttributeTypedArgument> outputTypeArgs = (IList<CustomAttributeTypedArgument>)outputTypeData.ConstructorArguments[0].Value;
            Assert.Contains(outputTypeArgs, a => (Type)a.Value == typeof(PSInfisicalAPI.Models.InfisicalCertificateResult));
        }

        [Fact]
        public void BuildResult_Splits_Chain_Into_Leaf_Intermediates_And_Root()
        {
            (string leafPem, _, string leafThumb) = PemCertificateBuilderTests.CreateSelfSignedExposed("BuildResult.Leaf");
            (string intermediatePem, _, string intermediateThumb) = PemCertificateBuilderTests.CreateSelfSignedExposed("BuildResult.Intermediate");
            (string rootPem, _, string rootThumb) = PemCertificateBuilderTests.CreateSelfSignedExposed("BuildResult.Root");

            PSInfisicalAPI.Models.InfisicalSignedCertificate signed = new PSInfisicalAPI.Models.InfisicalSignedCertificate
            {
                SerialNumber = "ABC123",
                CertificatePem = leafPem,
                CertificateChainPem = intermediatePem + rootPem,
                IssuingCaCertificatePem = rootPem
            };

            using (System.Security.Cryptography.X509Certificates.X509Certificate2 leaf = new System.Security.Cryptography.X509Certificates.X509Certificate2(System.Text.Encoding.ASCII.GetBytes(leafPem)))
            {
                Type helperType = ModuleAssembly.GetType("PSInfisicalAPI.Pki.InfisicalCertificateRequestHelpers", true);
                MethodInfo buildResult = helperType.GetMethod("BuildResult", BindingFlags.Public | BindingFlags.Static);
                Assert.NotNull(buildResult);

                PSInfisicalAPI.Models.InfisicalCertificateResult result = (PSInfisicalAPI.Models.InfisicalCertificateResult)buildResult.Invoke(null, new object[] { leaf, signed });

                Assert.Same(leaf, result.Leaf);
                Assert.Equal("ABC123", result.SerialNumber);
                Assert.Empty(result.Intermediates);
                Assert.NotNull(result.Root);
                Assert.Equal(2, result.Chain.Length);
                Assert.Same(leaf, result.Chain[0]);
            }
        }

        [Theory]
        [InlineData(InfisicalPrivateKeyProtection.LocalOnly, false, false, System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.DefaultKeySet)]
        [InlineData(InfisicalPrivateKeyProtection.Exportable, false, false, System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.Exportable)]
        [InlineData(InfisicalPrivateKeyProtection.NonExportable, false, false, System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.DefaultKeySet)]
        [InlineData(InfisicalPrivateKeyProtection.LocalOnly, true, false, System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.PersistKeySet)]
        [InlineData(InfisicalPrivateKeyProtection.LocalOnly, false, true, System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.MachineKeySet)]
        [InlineData(InfisicalPrivateKeyProtection.Exportable, true, true, System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.Exportable | System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.MachineKeySet | System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.PersistKeySet)]
        public void ResolveKeyStorageFlags_Maps_Protection_And_Switches(InfisicalPrivateKeyProtection protection, bool persist, bool machine, System.Security.Cryptography.X509Certificates.X509KeyStorageFlags expected)
        {
            Type helperType = ModuleAssembly.GetType("PSInfisicalAPI.Pki.InfisicalCertificateRequestHelpers", true);
            MethodInfo method = helperType.GetMethod("ResolveKeyStorageFlags", BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(method);

            System.Security.Cryptography.X509Certificates.X509KeyStorageFlags actual = (System.Security.Cryptography.X509Certificates.X509KeyStorageFlags)method.Invoke(null, new object[] { protection, persist, machine });
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(InfisicalPrivateKeyProtection.LocalOnly, false, false)]
        [InlineData(InfisicalPrivateKeyProtection.Exportable, false, false)]
        [InlineData(InfisicalPrivateKeyProtection.NonExportable, false, true)]
        [InlineData(InfisicalPrivateKeyProtection.Ephemeral, false, true)]
        [InlineData(InfisicalPrivateKeyProtection.LocalOnly, true, true)]
        [InlineData(InfisicalPrivateKeyProtection.Exportable, true, true)]
        public void ShouldScrubPrivateKeyPem_Returns_Expected(InfisicalPrivateKeyProtection protection, bool hasPath, bool expected)
        {
            Type helperType = ModuleAssembly.GetType("PSInfisicalAPI.Pki.InfisicalCertificateRequestHelpers", true);
            MethodInfo method = helperType.GetMethod("ShouldScrubPrivateKeyPem", BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(method);

            bool actual = (bool)method.Invoke(null, new object[] { protection, hasPath });
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void WritePrivateKeyPem_Writes_File_And_Creates_Directory()
        {
            Type helperType = ModuleAssembly.GetType("PSInfisicalAPI.Pki.InfisicalCertificateRequestHelpers", true);
            MethodInfo method = helperType.GetMethod("WritePrivateKeyPem", BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(method);

            string tempRoot = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "PSInfisicalAPI_PemWrite_" + Guid.NewGuid().ToString("N"));
            string nested = System.IO.Path.Combine(tempRoot, "nested", "key.pem");
            const string pem = "-----BEGIN PRIVATE KEY-----\nMIIBVgIBADANBgkqhkiG9w0BAQEFAA==\n-----END PRIVATE KEY-----\n";
            try
            {
                method.Invoke(null, new object[] { pem, nested });
                Assert.True(System.IO.File.Exists(nested));
                Assert.Equal(pem, System.IO.File.ReadAllText(nested));
            }
            finally
            {
                if (System.IO.Directory.Exists(tempRoot)) { System.IO.Directory.Delete(tempRoot, true); }
            }
        }

        [Fact]
        public void BuildResultFromExistingLocal_Populates_Leaf_And_Pem_For_Selfsigned()
        {
            (string leafPem, _, string leafThumb) = PemCertificateBuilderTests.CreateSelfSignedExposed("ReuseLookup.Leaf");

            using (System.Security.Cryptography.X509Certificates.X509Certificate2 leaf = new System.Security.Cryptography.X509Certificates.X509Certificate2(System.Text.Encoding.ASCII.GetBytes(leafPem)))
            {
                Type helperType = ModuleAssembly.GetType("PSInfisicalAPI.Pki.InfisicalCertificateRequestHelpers", true);
                MethodInfo build = helperType.GetMethod("BuildResultFromExistingLocal", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(System.Security.Cryptography.X509Certificates.X509Certificate2) }, null);
                Assert.NotNull(build);

                PSInfisicalAPI.Models.InfisicalCertificateResult result = (PSInfisicalAPI.Models.InfisicalCertificateResult)build.Invoke(null, new object[] { leaf });

                Assert.Same(leaf, result.Leaf);
                Assert.Equal(leaf.SerialNumber, result.SerialNumber);
                Assert.Contains("BEGIN CERTIFICATE", result.CertificatePem);
                Assert.NotNull(result.Chain);
                Assert.NotEmpty(result.Chain);
                Assert.Same(leaf, result.Chain[0]);
                Assert.Empty(result.Intermediates);
            }
        }

        [Fact]
        public void BuildResultFromExistingLocal_Has_Bundle_Fallback_Overload()
        {
            Type helperType = ModuleAssembly.GetType("PSInfisicalAPI.Pki.InfisicalCertificateRequestHelpers", true);
            MethodInfo overload = helperType.GetMethod(
                "BuildResultFromExistingLocal",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new Type[] { typeof(System.Security.Cryptography.X509Certificates.X509Certificate2), typeof(PSInfisicalAPI.Models.InfisicalCertificateBundle) },
                null);
            Assert.NotNull(overload);
            Assert.Equal(typeof(PSInfisicalAPI.Models.InfisicalCertificateResult), overload.ReturnType);
        }

        [Fact]
        public void BuildResultFromExistingLocal_With_Null_Bundle_Matches_LocalOnly_Behavior()
        {
            (string leafPem, _, _) = PemCertificateBuilderTests.CreateSelfSignedExposed("ReuseLookup.Bundle.Null.Leaf");

            using (System.Security.Cryptography.X509Certificates.X509Certificate2 leaf = new System.Security.Cryptography.X509Certificates.X509Certificate2(System.Text.Encoding.ASCII.GetBytes(leafPem)))
            {
                Type helperType = ModuleAssembly.GetType("PSInfisicalAPI.Pki.InfisicalCertificateRequestHelpers", true);
                MethodInfo overload = helperType.GetMethod(
                    "BuildResultFromExistingLocal",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new Type[] { typeof(System.Security.Cryptography.X509Certificates.X509Certificate2), typeof(PSInfisicalAPI.Models.InfisicalCertificateBundle) },
                    null);

                PSInfisicalAPI.Models.InfisicalCertificateResult result = (PSInfisicalAPI.Models.InfisicalCertificateResult)overload.Invoke(null, new object[] { leaf, null });

                Assert.Same(leaf, result.Leaf);
                Assert.Empty(result.Intermediates);
                Assert.Single(result.Chain);
            }
        }

        [Fact]
        public void BuildResultFromExistingLocal_With_Bundle_Merges_Chain_From_Bundle()
        {
            (string leafPem, _, string leafThumb) = PemCertificateBuilderTests.CreateSelfSignedExposed("ReuseLookup.Bundle.Leaf");
            (string caPem, _, string caThumb) = PemCertificateBuilderTests.CreateSelfSignedExposed("ReuseLookup.Bundle.Ca");

            using (System.Security.Cryptography.X509Certificates.X509Certificate2 leaf = new System.Security.Cryptography.X509Certificates.X509Certificate2(System.Text.Encoding.ASCII.GetBytes(leafPem)))
            {
                PSInfisicalAPI.Models.InfisicalCertificateBundle bundle = new PSInfisicalAPI.Models.InfisicalCertificateBundle
                {
                    SerialNumber = leaf.SerialNumber,
                    CertificatePem = leafPem,
                    CertificateChainPem = caPem
                };

                Type helperType = ModuleAssembly.GetType("PSInfisicalAPI.Pki.InfisicalCertificateRequestHelpers", true);
                MethodInfo overload = helperType.GetMethod(
                    "BuildResultFromExistingLocal",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new Type[] { typeof(System.Security.Cryptography.X509Certificates.X509Certificate2), typeof(PSInfisicalAPI.Models.InfisicalCertificateBundle) },
                    null);

                PSInfisicalAPI.Models.InfisicalCertificateResult result = (PSInfisicalAPI.Models.InfisicalCertificateResult)overload.Invoke(null, new object[] { leaf, bundle });

                Assert.Same(leaf, result.Leaf);
                Assert.NotNull(result.Root);
                Assert.Equal(caThumb, result.Root.Thumbprint);
                Assert.Equal(2, result.Chain.Length);
                Assert.Same(leaf, result.Chain[0]);
                Assert.Equal(caThumb, result.Chain[1].Thumbprint);

                Assert.NotNull(result.CertificateChainPem);
                Assert.Contains("BEGIN CERTIFICATE", result.CertificateChainPem);
            }
        }

        [Fact]
        public void GetChainCertificateTargetStore_SelfSigned_Returns_Root()
        {
            using (System.Security.Cryptography.RSA rsa = System.Security.Cryptography.RSA.Create(2048))
            {
                System.Security.Cryptography.X509Certificates.CertificateRequest request = new System.Security.Cryptography.X509Certificates.CertificateRequest(
                    "CN=ChainRouting.SelfSigned",
                    rsa,
                    System.Security.Cryptography.HashAlgorithmName.SHA256,
                    System.Security.Cryptography.RSASignaturePadding.Pkcs1);
                using (System.Security.Cryptography.X509Certificates.X509Certificate2 selfSigned = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow.AddDays(1)))
                {
                    Type helperType = ModuleAssembly.GetType("PSInfisicalAPI.Pki.InfisicalCertificateRequestHelpers", true);
                    MethodInfo classify = helperType.GetMethod("GetChainCertificateTargetStore", BindingFlags.Public | BindingFlags.Static);
                    Assert.NotNull(classify);

                    object result = classify.Invoke(null, new object[] { selfSigned });
                    Assert.Equal(System.Security.Cryptography.X509Certificates.StoreName.Root, result);
                }
            }
        }

        [Fact]
        public void GetChainCertificateTargetStore_NonSelfSigned_Returns_CertificateAuthority()
        {
            using (System.Security.Cryptography.RSA rootRsa = System.Security.Cryptography.RSA.Create(2048))
            using (System.Security.Cryptography.RSA intermediateRsa = System.Security.Cryptography.RSA.Create(2048))
            {
                DateTimeOffset notBefore = DateTimeOffset.UtcNow.AddMinutes(-5);
                DateTimeOffset notAfter  = DateTimeOffset.UtcNow.AddDays(1);

                System.Security.Cryptography.X509Certificates.CertificateRequest rootRequest = new System.Security.Cryptography.X509Certificates.CertificateRequest(
                    "CN=ChainRouting.Root",
                    rootRsa,
                    System.Security.Cryptography.HashAlgorithmName.SHA256,
                    System.Security.Cryptography.RSASignaturePadding.Pkcs1);
                rootRequest.CertificateExtensions.Add(new System.Security.Cryptography.X509Certificates.X509BasicConstraintsExtension(true, false, 0, true));

                using (System.Security.Cryptography.X509Certificates.X509Certificate2 rootCert = rootRequest.CreateSelfSigned(notBefore, notAfter))
                {
                    System.Security.Cryptography.X509Certificates.CertificateRequest intermediateRequest = new System.Security.Cryptography.X509Certificates.CertificateRequest(
                        "CN=ChainRouting.Intermediate",
                        intermediateRsa,
                        System.Security.Cryptography.HashAlgorithmName.SHA256,
                        System.Security.Cryptography.RSASignaturePadding.Pkcs1);
                    intermediateRequest.CertificateExtensions.Add(new System.Security.Cryptography.X509Certificates.X509BasicConstraintsExtension(true, false, 0, true));

                    byte[] serial = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
                    using (System.Security.Cryptography.X509Certificates.X509Certificate2 intermediate = intermediateRequest.Create(rootCert, notBefore, notAfter, serial))
                    {
                        Assert.NotEqual(intermediate.Subject, intermediate.Issuer);

                        Type helperType = ModuleAssembly.GetType("PSInfisicalAPI.Pki.InfisicalCertificateRequestHelpers", true);
                        MethodInfo classify = helperType.GetMethod("GetChainCertificateTargetStore", BindingFlags.Public | BindingFlags.Static);

                        object result = classify.Invoke(null, new object[] { intermediate });
                        Assert.Equal(System.Security.Cryptography.X509Certificates.StoreName.CertificateAuthority, result);
                    }
                }
            }
        }

        [Fact]
        public void InstallChain_Has_X509Collection_Overload()
        {
            Type helperType = ModuleAssembly.GetType("PSInfisicalAPI.Pki.InfisicalCertificateRequestHelpers", true);
            Type loggerType = ModuleAssembly.GetType("PSInfisicalAPI.Logging.IInfisicalLogger", true);

            MethodInfo overload = helperType.GetMethod(
                "InstallChain",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new Type[]
                {
                    typeof(System.Collections.Generic.IEnumerable<System.Security.Cryptography.X509Certificates.X509Certificate2>),
                    typeof(System.Security.Cryptography.X509Certificates.StoreLocation),
                    typeof(bool),
                    loggerType,
                    typeof(string)
                },
                null);

            Assert.NotNull(overload);
            Assert.Equal(typeof(void), overload.ReturnType);
        }

        [Fact]
        public void InstallInfisicalCertificateCmdlet_Uses_ChainRouting_Helper()
        {
            Type cmdletType = ModuleAssembly.GetType("PSInfisicalAPI.Cmdlets.InstallInfisicalCertificateCmdlet", true);
            Assert.NotNull(cmdletType);

            Type helperType = ModuleAssembly.GetType("PSInfisicalAPI.Pki.InfisicalCertificateRequestHelpers", true);
            MethodInfo classify = helperType.GetMethod("GetChainCertificateTargetStore", BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(classify);
        }
    }
}
