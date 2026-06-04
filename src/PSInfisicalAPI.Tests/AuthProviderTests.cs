using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using PSInfisicalAPI.Authentication;
using PSInfisicalAPI.Errors;
using PSInfisicalAPI.Http;
using PSInfisicalAPI.Security;
using Xunit;

namespace PSInfisicalAPI.Tests
{
    public class AuthProviderTests
    {
        private sealed class CapturingHttpClient : IInfisicalHttpClient
        {
            public InfisicalHttpRequest CapturedRequest { get; private set; }
            public string ResponseBody { get; set; } = "{\"accessToken\":\"abc.def.ghi\",\"expiresIn\":3600,\"tokenType\":\"Bearer\"}";

            public InfisicalHttpResponse Send(InfisicalHttpRequest request)
            {
                CapturedRequest = request;
                return new InfisicalHttpResponse
                {
                    StatusCode = 200,
                    Body = ResponseBody,
                    Headers = new Dictionary<string, string>()
                };
            }
        }

        private static InfisicalAuthenticationRequest BaseRequest()
        {
            return new InfisicalAuthenticationRequest
            {
                BaseUri = new Uri("https://example.invalid"),
                ApiVersion = "v1"
            };
        }

        [Fact]
        public void JwtAuthProvider_Posts_IdentityId_And_Jwt()
        {
            CapturingHttpClient http = new CapturingHttpClient();
            InfisicalAuthenticationRequest request = BaseRequest();
            request.IdentityId = "identity-1";
            request.Jwt = SecureStringUtility.ToReadOnlySecureString("token.value");

            InfisicalAuthenticationResult result = new JwtAuthProvider().Authenticate(request, http, null);

            Assert.NotNull(result);
            Assert.NotNull(http.CapturedRequest);
            Assert.Equal("POST", http.CapturedRequest.Method);
            Assert.EndsWith("/api/v1/auth/jwt-auth/login", http.CapturedRequest.Uri.AbsolutePath);
            JObject body = JObject.Parse(http.CapturedRequest.Body);
            Assert.Equal("identity-1", (string)body["identityId"]);
            Assert.Equal("token.value", (string)body["jwt"]);
        }

        [Fact]
        public void OidcAuthProvider_Posts_IdentityId_And_Jwt_To_Oidc_Endpoint()
        {
            CapturingHttpClient http = new CapturingHttpClient();
            InfisicalAuthenticationRequest request = BaseRequest();
            request.IdentityId = "identity-2";
            request.Jwt = SecureStringUtility.ToReadOnlySecureString("oidc.token");

            new OidcAuthProvider().Authenticate(request, http, null);

            Assert.EndsWith("/api/v1/auth/oidc-auth/login", http.CapturedRequest.Uri.AbsolutePath);
            JObject body = JObject.Parse(http.CapturedRequest.Body);
            Assert.Equal("identity-2", (string)body["identityId"]);
            Assert.Equal("oidc.token", (string)body["jwt"]);
        }

        [Fact]
        public void LdapAuthProvider_Posts_Username_And_Password_To_Ldap_Endpoint()
        {
            CapturingHttpClient http = new CapturingHttpClient();
            InfisicalAuthenticationRequest request = BaseRequest();
            request.Username = "svc.account";
            request.Password = SecureStringUtility.ToReadOnlySecureString("P@ssw0rd!");

            new LdapAuthProvider().Authenticate(request, http, null);

            Assert.EndsWith("/api/v1/auth/ldap-auth/login", http.CapturedRequest.Uri.AbsolutePath);
            JObject body = JObject.Parse(http.CapturedRequest.Body);
            Assert.Equal("svc.account", (string)body["username"]);
            Assert.Equal("P@ssw0rd!", (string)body["password"]);
            Assert.False(body.ContainsKey("identityId"));
        }

        [Fact]
        public void LdapAuthProvider_Includes_IdentityId_When_Supplied()
        {
            CapturingHttpClient http = new CapturingHttpClient();
            InfisicalAuthenticationRequest request = BaseRequest();
            request.Username = "u";
            request.Password = SecureStringUtility.ToReadOnlySecureString("p");
            request.IdentityId = "id-ldap";

            new LdapAuthProvider().Authenticate(request, http, null);

            JObject body = JObject.Parse(http.CapturedRequest.Body);
            Assert.Equal("id-ldap", (string)body["identityId"]);
        }

        [Fact]
        public void AzureAuthProvider_Posts_IdentityId_And_Jwt_To_Azure_Endpoint()
        {
            CapturingHttpClient http = new CapturingHttpClient();
            InfisicalAuthenticationRequest request = BaseRequest();
            request.IdentityId = "identity-az";
            request.Jwt = SecureStringUtility.ToReadOnlySecureString("az.token");

            new AzureAuthProvider().Authenticate(request, http, null);

            Assert.EndsWith("/api/v1/auth/azure-auth/login", http.CapturedRequest.Uri.AbsolutePath);
            JObject body = JObject.Parse(http.CapturedRequest.Body);
            Assert.Equal("identity-az", (string)body["identityId"]);
            Assert.Equal("az.token", (string)body["jwt"]);
        }

        [Fact]
        public void GcpIamAuthProvider_Posts_IdentityId_And_Jwt_To_Gcp_Endpoint()
        {
            CapturingHttpClient http = new CapturingHttpClient();
            InfisicalAuthenticationRequest request = BaseRequest();
            request.IdentityId = "identity-gcp";
            request.Jwt = SecureStringUtility.ToReadOnlySecureString("gcp.token");

            new GcpIamAuthProvider().Authenticate(request, http, null);

            Assert.EndsWith("/api/v1/auth/gcp-auth/login", http.CapturedRequest.Uri.AbsolutePath);
            JObject body = JObject.Parse(http.CapturedRequest.Body);
            Assert.Equal("identity-gcp", (string)body["identityId"]);
            Assert.Equal("gcp.token", (string)body["jwt"]);
        }

        [Fact]
        public void JwtAuthProvider_Throws_When_IdentityId_Missing()
        {
            InfisicalAuthenticationRequest request = BaseRequest();
            request.Jwt = SecureStringUtility.ToReadOnlySecureString("x");
            Assert.Throws<InfisicalAuthenticationException>(() =>
                new JwtAuthProvider().Authenticate(request, new CapturingHttpClient(), null));
        }

        [Fact]
        public void LdapAuthProvider_Throws_When_Password_Missing()
        {
            InfisicalAuthenticationRequest request = BaseRequest();
            request.Username = "u";
            Assert.Throws<InfisicalAuthenticationException>(() =>
                new LdapAuthProvider().Authenticate(request, new CapturingHttpClient(), null));
        }
    }
}
