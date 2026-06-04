using System.Collections.Generic;
using PSInfisicalAPI.Endpoints;
using PSInfisicalAPI.Errors;
using PSInfisicalAPI.Http;
using PSInfisicalAPI.Logging;
using PSInfisicalAPI.Security;

namespace PSInfisicalAPI.Authentication
{
    public sealed class OidcAuthProvider : IInfisicalAuthProvider
    {
        private const string Component = "OidcAuthProvider";

        public string Name { get { return "OidcAuth"; } }

        public InfisicalAuthenticationResult Authenticate(InfisicalAuthenticationRequest request, IInfisicalHttpClient httpClient, IInfisicalLogger logger)
        {
            if (request == null || string.IsNullOrEmpty(request.IdentityId))
            {
                throw new InfisicalAuthenticationException("IdentityId is required for OIDC Auth.");
            }

            if (request.Jwt == null || request.Jwt.Length == 0)
            {
                throw new InfisicalAuthenticationException("Jwt is required for OIDC Auth.");
            }

            return IdentityLoginExecutor.Execute(InfisicalEndpointNames.OidcAuthLogin, Component, request, httpClient, logger, serializer =>
            {
                return SecureStringUtility.UsePlainText(request.Jwt, plainJwt =>
                {
                    Dictionary<string, string> bodyObject = new Dictionary<string, string>
                    {
                        { "identityId", request.IdentityId },
                        { "jwt", plainJwt ?? string.Empty }
                    };

                    return serializer.Serialize(bodyObject);
                });
            });
        }
    }
}
