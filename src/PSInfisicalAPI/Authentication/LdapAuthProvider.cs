using System.Collections.Generic;
using PSInfisicalAPI.Endpoints;
using PSInfisicalAPI.Errors;
using PSInfisicalAPI.Http;
using PSInfisicalAPI.Logging;
using PSInfisicalAPI.Security;

namespace PSInfisicalAPI.Authentication
{
    public sealed class LdapAuthProvider : IInfisicalAuthProvider
    {
        private const string Component = "LdapAuthProvider";

        public string Name { get { return "LdapAuth"; } }

        public InfisicalAuthenticationResult Authenticate(InfisicalAuthenticationRequest request, IInfisicalHttpClient httpClient, IInfisicalLogger logger)
        {
            if (request == null || string.IsNullOrEmpty(request.Username))
            {
                throw new InfisicalAuthenticationException("Username is required for LDAP Auth.");
            }

            if (request.Password == null || request.Password.Length == 0)
            {
                throw new InfisicalAuthenticationException("Password is required for LDAP Auth.");
            }

            return IdentityLoginExecutor.Execute(InfisicalEndpointNames.LdapAuthLogin, Component, request, httpClient, logger, serializer =>
            {
                return SecureStringUtility.UsePlainText(request.Password, plainPassword =>
                {
                    Dictionary<string, string> bodyObject = new Dictionary<string, string>
                    {
                        { "username", request.Username },
                        { "password", plainPassword ?? string.Empty }
                    };

                    if (!string.IsNullOrEmpty(request.IdentityId))
                    {
                        bodyObject["identityId"] = request.IdentityId;
                    }

                    return serializer.Serialize(bodyObject);
                });
            });
        }
    }
}
