using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Security;
using PSInfisicalAPI.Authentication;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Errors;
using PSInfisicalAPI.Models;

namespace PSInfisicalAPI.Cmdlets
{
    [Cmdlet(VerbsCommunications.Connect, "Infisical", DefaultParameterSetName = "UniversalAuth")]
    [OutputType(typeof(InfisicalConnection))]
    public sealed class ConnectInfisicalCmdlet : InfisicalCmdletBase
    {
        private const string ParameterSetUniversalAuth = "UniversalAuth";
        private const string ParameterSetToken = "Token";
        private const string ParameterSetJwt = "JwtAuth";
        private const string ParameterSetOidc = "OidcAuth";
        private const string ParameterSetLdap = "LdapAuth";
        private const string ParameterSetAzure = "AzureAuth";
        private const string ParameterSetGcpIam = "GcpIamAuth";
        private const string Component = "ConnectInfisicalCmdlet";

        [Parameter]
        public Uri BaseUri { get; set; }

        [Parameter]
        public string OrganizationId { get; set; }

        [Parameter(ParameterSetName = ParameterSetUniversalAuth)]
        public string ClientId { get; set; }

        [Parameter(ParameterSetName = ParameterSetUniversalAuth)]
        public SecureString ClientSecret { get; set; }

        [Parameter(ParameterSetName = ParameterSetToken)]
        public SecureString AccessToken { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = ParameterSetJwt)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSetOidc)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSetAzure)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSetGcpIam)]
        [Parameter(ParameterSetName = ParameterSetLdap)]
        public string IdentityId { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = ParameterSetJwt)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSetOidc)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSetAzure)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSetGcpIam)]
        public SecureString Jwt { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = ParameterSetLdap)]
        public string Username { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = ParameterSetLdap)]
        public SecureString Password { get; set; }

        [Parameter]
        public string ApiVersion { get; set; } = "v4";

        [Parameter]
        public SwitchParameter PassThru { get; set; }

        [Parameter]
        public SwitchParameter SkipCertificateCheck { get; set; }

        [Parameter]
        public SwitchParameter AllowInsecureTransport { get; set; }

        protected override bool ShouldSkipCertificateCheck()
        {
            return SkipCertificateCheck.IsPresent;
        }

        protected override void ProcessRecord()
        {
            try
            {
                ResolveMissingParametersFromEnvironment();
                ValidateRequiredParameters();
                ValidateTransportSafety();

                IInfisicalAuthProvider provider;
                InfisicalAuthenticationRequest request;
                InfisicalAuthType authType;

                switch (ParameterSetName)
                {
                    case ParameterSetToken:
                        provider = new TokenAuthProvider();
                        authType = InfisicalAuthType.Token;
                        request = new InfisicalAuthenticationRequest
                        {
                            BaseUri = BaseUri,
                            ApiVersion = ApiVersion,
                            PreSuppliedAccessToken = AccessToken
                        };
                        break;

                    case ParameterSetJwt:
                        provider = new JwtAuthProvider();
                        authType = InfisicalAuthType.Jwt;
                        request = new InfisicalAuthenticationRequest
                        {
                            BaseUri = BaseUri,
                            ApiVersion = ApiVersion,
                            IdentityId = IdentityId,
                            Jwt = Jwt
                        };
                        break;

                    case ParameterSetOidc:
                        provider = new OidcAuthProvider();
                        authType = InfisicalAuthType.Oidc;
                        request = new InfisicalAuthenticationRequest
                        {
                            BaseUri = BaseUri,
                            ApiVersion = ApiVersion,
                            IdentityId = IdentityId,
                            Jwt = Jwt
                        };
                        break;

                    case ParameterSetLdap:
                        provider = new LdapAuthProvider();
                        authType = InfisicalAuthType.Ldap;
                        request = new InfisicalAuthenticationRequest
                        {
                            BaseUri = BaseUri,
                            ApiVersion = ApiVersion,
                            IdentityId = IdentityId,
                            Username = Username,
                            Password = Password
                        };
                        break;

                    case ParameterSetAzure:
                        provider = new AzureAuthProvider();
                        authType = InfisicalAuthType.Azure;
                        request = new InfisicalAuthenticationRequest
                        {
                            BaseUri = BaseUri,
                            ApiVersion = ApiVersion,
                            IdentityId = IdentityId,
                            Jwt = Jwt
                        };
                        break;

                    case ParameterSetGcpIam:
                        provider = new GcpIamAuthProvider();
                        authType = InfisicalAuthType.GcpIam;
                        request = new InfisicalAuthenticationRequest
                        {
                            BaseUri = BaseUri,
                            ApiVersion = ApiVersion,
                            IdentityId = IdentityId,
                            Jwt = Jwt
                        };
                        break;

                    default:
                        provider = new UniversalAuthProvider();
                        authType = InfisicalAuthType.UniversalAuth;
                        request = new InfisicalAuthenticationRequest
                        {
                            BaseUri = BaseUri,
                            ApiVersion = ApiVersion,
                            ClientId = ClientId,
                            ClientSecret = ClientSecret
                        };
                        break;
                }

                InfisicalAuthenticationResult authResult = provider.Authenticate(request, HttpClient, Logger);

                if (authResult == null || authResult.AccessToken == null)
                {
                    throw new InfisicalAuthenticationException("Authentication did not produce an access token.");
                }

                bool apiVersionExplicitlyBound = MyInvocation.BoundParameters.ContainsKey("ApiVersion");

                InfisicalConnection connection = new InfisicalConnection
                {
                    BaseUri = BaseUri,
                    ApiVersion = ApiVersion,
                    PinnedApiVersion = apiVersionExplicitlyBound ? ApiVersion : null,
                    AuthType = authType,
                    OrganizationId = OrganizationId,
                    ConnectedAtUtc = DateTimeOffset.UtcNow,
                    ExpiresAtUtc = authResult.ExpiresAtUtc,
                    IsConnected = true,
                    AccessToken = authResult.AccessToken,
                    SkipCertificateCheck = SkipCertificateCheck.IsPresent,
                    AllowInsecureTransport = AllowInsecureTransport.IsPresent
                };

                InfisicalSessionManager.SetCurrent(connection);

                if (PassThru.IsPresent)
                {
                    WriteObject(connection);
                }
            }
            catch (Exception exception)
            {
                ThrowTerminatingForException(Component, "Connect", exception);
            }
        }

        private void ValidateTransportSafety()
        {
            bool isHttp = BaseUri != null && string.Equals(BaseUri.Scheme, "http", StringComparison.OrdinalIgnoreCase);

            if (isHttp && !AllowInsecureTransport.IsPresent)
            {
                throw new InfisicalConfigurationException("BaseUri '" + BaseUri + "' is not HTTPS. Re-run Connect-Infisical with -AllowInsecureTransport to permit plaintext.");
            }

            if (SkipCertificateCheck.IsPresent)
            {
                Logger.Warning(Component, "SkipCertificateCheck is enabled. TLS certificate validation is disabled for this session. Do not use in production.");
            }

            if (AllowInsecureTransport.IsPresent && isHttp)
            {
                Logger.Warning(Component, "AllowInsecureTransport is enabled and BaseUri uses HTTP. Credentials and secrets will traverse the network unencrypted. Do not use in production.");
            }
        }

        private void ResolveMissingParametersFromEnvironment()
        {
            bool tokenSet = string.Equals(ParameterSetName, ParameterSetToken, StringComparison.Ordinal);
            bool universalSet = string.Equals(ParameterSetName, ParameterSetUniversalAuth, StringComparison.Ordinal);

            bool needsScan =
                BaseUri == null ||
                string.IsNullOrWhiteSpace(OrganizationId) ||
                (tokenSet && (AccessToken == null || AccessToken.Length == 0)) ||
                (universalSet && string.IsNullOrWhiteSpace(ClientId)) ||
                (universalSet && (ClientSecret == null || ClientSecret.Length == 0));

            if (!needsScan)
            {
                return;
            }

            Logger.Verbose(Component, "Attempting to resolve missing Infisical connection parameters from environment variables. Please Wait...");

            if (BaseUri == null)
            {
                string resolved = InfisicalEnvironmentResolver.ResolveString("BaseUri", InfisicalEnvironmentResolver.BaseUriPatterns, null, Logger);
                if (!string.IsNullOrWhiteSpace(resolved))
                {
                    Uri parsed;
                    if (Uri.TryCreate(resolved, UriKind.Absolute, out parsed))
                    {
                        BaseUri = parsed;
                    }
                }
            }

            OrganizationId = InfisicalEnvironmentResolver.ResolveString("OrganizationId", InfisicalEnvironmentResolver.OrganizationIdPatterns, OrganizationId, Logger);

            if (tokenSet)
            {
                AccessToken = InfisicalEnvironmentResolver.ResolveSecureString("AccessToken", InfisicalEnvironmentResolver.AccessTokenPatterns, AccessToken, Logger);
            }
            else if (universalSet)
            {
                ClientId = InfisicalEnvironmentResolver.ResolveString("ClientId", InfisicalEnvironmentResolver.ClientIdPatterns, ClientId, Logger);
                ClientSecret = InfisicalEnvironmentResolver.ResolveSecureString("ClientSecret", InfisicalEnvironmentResolver.ClientSecretPatterns, ClientSecret, Logger);
            }

            if (!MyInvocation.BoundParameters.ContainsKey("ApiVersion"))
            {
                string resolvedVersion = InfisicalEnvironmentResolver.ResolveString("ApiVersion", InfisicalEnvironmentResolver.ApiVersionPatterns, null, Logger);
                if (!string.IsNullOrWhiteSpace(resolvedVersion))
                {
                    ApiVersion = resolvedVersion;
                }
            }
        }

        private void ValidateRequiredParameters()
        {
            List<string> missing = new List<string>();

            if (BaseUri == null) { missing.Add("BaseUri"); }
            if (string.IsNullOrWhiteSpace(OrganizationId)) { missing.Add("OrganizationId"); }

            if (string.Equals(ParameterSetName, ParameterSetToken, StringComparison.Ordinal))
            {
                if (AccessToken == null || AccessToken.Length == 0) { missing.Add("AccessToken"); }
            }
            else if (string.Equals(ParameterSetName, ParameterSetUniversalAuth, StringComparison.Ordinal))
            {
                if (string.IsNullOrWhiteSpace(ClientId)) { missing.Add("ClientId"); }
                if (ClientSecret == null || ClientSecret.Length == 0) { missing.Add("ClientSecret"); }
            }

            if (missing.Count > 0)
            {
                throw new InfisicalConfigurationException(string.Format(
                    "Required Connect-Infisical parameter(s) not supplied and no matching environment variables were found: {0}.",
                    string.Join(", ", missing.ToArray())));
            }
        }
    }
}
