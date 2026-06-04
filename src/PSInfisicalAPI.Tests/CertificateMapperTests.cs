using System;
using System.Reflection;
using PSInfisicalAPI.Models;
using Xunit;

namespace PSInfisicalAPI.Tests
{
    public class CertificateMapperTests
    {
        private static readonly Assembly ModuleAssembly = typeof(PSInfisicalAPI.Connections.InfisicalConnection).Assembly;
        private static readonly Type CertMapperType = ModuleAssembly.GetType("PSInfisicalAPI.Pki.InfisicalCertificateMapper", true);
        private static readonly Type CertDtoType = ModuleAssembly.GetType("PSInfisicalAPI.Pki.InfisicalCertificateResponseDto", true);
        private static readonly Type CaMapperType = ModuleAssembly.GetType("PSInfisicalAPI.Pki.InfisicalCaMapper", true);
        private static readonly Type CaDtoType = ModuleAssembly.GetType("PSInfisicalAPI.Pki.InfisicalInternalCaResponseDto", true);
        private static readonly Type BundleDtoType = ModuleAssembly.GetType("PSInfisicalAPI.Pki.InfisicalCertificateBundleResponseDto", true);

        private static InfisicalCertificate InvokeCertMap(object dto, string fallbackProjectId)
        {
            MethodInfo map = CertMapperType.GetMethod("Map", BindingFlags.Public | BindingFlags.Static);
            return (InfisicalCertificate)map.Invoke(null, new object[] { dto, fallbackProjectId });
        }

        private static InfisicalCertificateAuthority InvokeCaMap(object dto, string fallbackProjectId)
        {
            MethodInfo map = CaMapperType.GetMethod("Map", BindingFlags.Public | BindingFlags.Static);
            return (InfisicalCertificateAuthority)map.Invoke(null, new object[] { dto, fallbackProjectId });
        }

        private static InfisicalCertificateBundle InvokeBundleMap(object dto)
        {
            MethodInfo map = CertMapperType.GetMethod("MapBundle", BindingFlags.Public | BindingFlags.Static);
            return (InfisicalCertificateBundle)map.Invoke(null, new object[] { dto });
        }

        [Fact]
        public void CertificateMap_Null_Returns_Null()
        {
            Assert.Null(InvokeCertMap(null, "proj-x"));
        }

        [Fact]
        public void CertificateMap_Populates_Fields_And_Parses_Timestamps()
        {
            object dto = Activator.CreateInstance(CertDtoType);
            CertDtoType.GetProperty("Id").SetValue(dto, "cert-1");
            CertDtoType.GetProperty("SerialNumber").SetValue(dto, "AABBCC");
            CertDtoType.GetProperty("CommonName").SetValue(dto, "example.com");
            CertDtoType.GetProperty("FriendlyName").SetValue(dto, "Example");
            CertDtoType.GetProperty("HasPrivateKey").SetValue(dto, true);
            CertDtoType.GetProperty("NotAfter").SetValue(dto, "2030-01-02T03:04:05Z");

            InfisicalCertificate mapped = InvokeCertMap(dto, "proj-fallback");
            Assert.Equal("cert-1", mapped.Id);
            Assert.Equal("AABBCC", mapped.SerialNumber);
            Assert.Equal("example.com", mapped.CommonName);
            Assert.Equal("Example", mapped.FriendlyName);
            Assert.True(mapped.HasPrivateKey);
            Assert.Equal("proj-fallback", mapped.ProjectId);
            Assert.True(mapped.NotAfterUtc.HasValue);
            Assert.Equal(new DateTimeOffset(2030, 1, 2, 3, 4, 5, TimeSpan.Zero), mapped.NotAfterUtc.Value);
        }

        [Fact]
        public void CertificateMap_Explicit_ProjectId_Wins_Over_Fallback()
        {
            object dto = Activator.CreateInstance(CertDtoType);
            CertDtoType.GetProperty("Id").SetValue(dto, "cert-2");
            CertDtoType.GetProperty("ProjectId").SetValue(dto, "proj-real");

            InfisicalCertificate mapped = InvokeCertMap(dto, "proj-fallback");
            Assert.Equal("proj-real", mapped.ProjectId);
        }

        [Fact]
        public void CaMap_Populates_Fields()
        {
            object dto = Activator.CreateInstance(CaDtoType);
            CaDtoType.GetProperty("Id").SetValue(dto, "ca-1");
            CaDtoType.GetProperty("Name").SetValue(dto, "internal-root");
            CaDtoType.GetProperty("Type").SetValue(dto, "internal");
            CaDtoType.GetProperty("Status").SetValue(dto, "active");
            CaDtoType.GetProperty("CommonName").SetValue(dto, "Internal Root CA");

            InfisicalCertificateAuthority mapped = InvokeCaMap(dto, "proj-fallback");
            Assert.Equal("ca-1", mapped.Id);
            Assert.Equal("internal-root", mapped.Name);
            Assert.Equal("internal", mapped.Type);
            Assert.Equal("active", mapped.Status);
            Assert.Equal("Internal Root CA", mapped.CommonName);
            Assert.Equal("proj-fallback", mapped.ProjectId);
        }

        [Fact]
        public void BundleMap_Maps_All_Pem_Fields()
        {
            object dto = Activator.CreateInstance(BundleDtoType);
            BundleDtoType.GetProperty("SerialNumber").SetValue(dto, "AABBCC");
            BundleDtoType.GetProperty("Certificate").SetValue(dto, "CERT-PEM");
            BundleDtoType.GetProperty("CertificateChain").SetValue(dto, "CHAIN-PEM");
            BundleDtoType.GetProperty("PrivateKey").SetValue(dto, "KEY-PEM");

            InfisicalCertificateBundle mapped = InvokeBundleMap(dto);
            Assert.Equal("AABBCC", mapped.SerialNumber);
            Assert.Equal("CERT-PEM", mapped.CertificatePem);
            Assert.Equal("CHAIN-PEM", mapped.CertificateChainPem);
            Assert.Equal("KEY-PEM", mapped.PrivateKeyPem);
        }
    }
}
