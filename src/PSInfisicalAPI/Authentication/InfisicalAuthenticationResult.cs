using System;
using System.Security;

namespace PSInfisicalAPI.Authentication
{
    public sealed class InfisicalAuthenticationResult
    {
        public SecureString AccessToken { get; set; }
        public DateTimeOffset? ExpiresAtUtc { get; set; }
        public string TokenType { get; set; }
    }
}
