using System;
using System.Collections.Generic;
using System.Security;
using PSInfisicalAPI.Models;

namespace PSInfisicalAPI.Connections
{
    public sealed class InfisicalConnection
    {
        public Uri BaseUri { get; set; }
        public string ApiVersion { get; set; }
        public string PinnedApiVersion { get; set; }
        public InfisicalAuthType AuthType { get; set; }
        public string OrganizationId { get; set; }
        public DateTimeOffset ConnectedAtUtc { get; set; }
        public DateTimeOffset? ExpiresAtUtc { get; set; }
        public bool IsConnected { get; set; }

        public Dictionary<string, string> ResolvedEndpointVersions { get; } = new Dictionary<string, string>(StringComparer.Ordinal);

        internal SecureString AccessToken { get; set; }

        public override string ToString()
        {
            return string.Concat(
                "BaseUri=", BaseUri != null ? BaseUri.ToString() : "",
                " AuthType=", AuthType.ToString(),
                " Connected=", IsConnected ? "true" : "false");
        }
    }
}
