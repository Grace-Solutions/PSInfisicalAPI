using System;
using System.Security;

namespace PSInfisicalAPI.Authentication
{
    public sealed class InfisicalAuthenticationRequest
    {
        public Uri BaseUri { get; set; }
        public string ApiVersion { get; set; }
        public string ClientId { get; set; }
        public SecureString ClientSecret { get; set; }
        public SecureString PreSuppliedAccessToken { get; set; }

        public string IdentityId { get; set; }
        public SecureString Jwt { get; set; }
        public string Username { get; set; }
        public SecureString Password { get; set; }
    }
}
