using System;
using System.Collections;
using System.Reflection;
using PSInfisicalAPI.Http;
using PSInfisicalAPI.Logging;
using PSInfisicalAPI.Pki;
using Xunit;

namespace PSInfisicalAPI.Tests
{
    public class PkiClientParseTests
    {
        private sealed class NoopHttpClient : IInfisicalHttpClient
        {
            public InfisicalHttpResponse Send(InfisicalHttpRequest request) { throw new NotImplementedException(); }
        }

        private static InfisicalPkiClient CreateClient()
        {
            return new InfisicalPkiClient(new NoopHttpClient(), NullInfisicalLogger.Instance);
        }

        private static object InvokeNonPublic(InfisicalPkiClient client, string methodName, string body)
        {
            MethodInfo method = typeof(InfisicalPkiClient).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            return method.Invoke(client, new object[] { body });
        }

        [Fact]
        public void ParseCaListBody_Reads_Raw_Json_Array()
        {
            string body = "[{\"id\":\"ca-1\",\"name\":\"intermediate\",\"projectId\":\"p1\",\"configuration\":{\"commonName\":\"Intermediate CA\",\"keyAlgorithm\":\"RSA_2048\"}}]";
            object result = InvokeNonPublic(CreateClient(), "ParseCaListBody", body);
            IList list = (IList)result;
            Assert.Single(list);
            object dto = list[0];
            Assert.Equal("ca-1", dto.GetType().GetProperty("Id").GetValue(dto));
            object cfg = dto.GetType().GetProperty("Configuration").GetValue(dto);
            Assert.NotNull(cfg);
            Assert.Equal("Intermediate CA", cfg.GetType().GetProperty("CommonName").GetValue(cfg));
        }

        [Fact]
        public void ParseCaListBody_Reads_CertificateAuthorities_Wrapper()
        {
            string body = "{\"certificateAuthorities\":[{\"id\":\"ca-2\",\"name\":\"root\"}]}";
            object result = InvokeNonPublic(CreateClient(), "ParseCaListBody", body);
            IList list = (IList)result;
            Assert.Single(list);
            object dto = list[0];
            Assert.Equal("ca-2", dto.GetType().GetProperty("Id").GetValue(dto));
        }

        [Fact]
        public void ParseCaSingleBody_Reads_Raw_Object_With_Configuration()
        {
            string body = "{\"id\":\"ca-9\",\"name\":\"intermediate-ca\",\"status\":\"active\",\"configuration\":{\"commonName\":\"GSPA Intermediate\",\"organization\":\"GSPA\"}}";
            object result = InvokeNonPublic(CreateClient(), "ParseCaSingleBody", body);
            Assert.NotNull(result);
            Assert.Equal("ca-9", result.GetType().GetProperty("Id").GetValue(result));
            object cfg = result.GetType().GetProperty("Configuration").GetValue(result);
            Assert.NotNull(cfg);
            Assert.Equal("GSPA Intermediate", cfg.GetType().GetProperty("CommonName").GetValue(cfg));
            Assert.Equal("GSPA", cfg.GetType().GetProperty("OrganizationName").GetValue(cfg));
        }

        [Fact]
        public void ParseCaSingleBody_Reads_CertificateAuthority_Wrapper()
        {
            string body = "{\"certificateAuthority\":{\"id\":\"ca-7\",\"name\":\"root\"}}";
            object result = InvokeNonPublic(CreateClient(), "ParseCaSingleBody", body);
            Assert.NotNull(result);
            Assert.Equal("ca-7", result.GetType().GetProperty("Id").GetValue(result));
        }

        [Fact]
        public void ParseCertificateSingleBody_Reads_Certificate_Wrapper()
        {
            string body = "{\"certificate\":{\"id\":\"cert-1\",\"serialNumber\":\"ABCD\",\"commonName\":\"host.example\"}}";
            object result = InvokeNonPublic(CreateClient(), "ParseCertificateSingleBody", body);
            Assert.NotNull(result);
            Assert.Equal("cert-1", result.GetType().GetProperty("Id").GetValue(result));
            Assert.Equal("ABCD", result.GetType().GetProperty("SerialNumber").GetValue(result));
        }
    }
}
