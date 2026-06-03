using System.Collections.Generic;
using PSInfisicalAPI.Endpoints;
using PSInfisicalAPI.Errors;
using PSInfisicalAPI.Http;
using PSInfisicalAPI.Logging;
using PSInfisicalAPI.Security;

namespace PSInfisicalAPI.Authentication
{
    public sealed class GcpIamAuthProvider : IInfisicalAuthProvider
    {
        private const string Component = "GcpIamAuthProvider";

        public string Name { get { return "GcpIamAuth"; } }

        public InfisicalAuthenticationResult Authenticate(InfisicalAuthenticationRequest request, IInfisicalHttpClient httpClient, IInfisicalLogger logger)
        {
            if (request == null || string.IsNullOrEmpty(request.IdentityId))
            {
                throw new InfisicalAuthenticationException("IdentityId is required for GCP IAM Auth.");
            }

            if (request.Jwt == null || request.Jwt.Length == 0)
            {
                throw new InfisicalAuthenticationException("Jwt is required for GCP IAM Auth.");
            }

            return IdentityLoginExecutor.Execute(InfisicalEndpointNames.GcpIamAuthLogin, Component, request, httpClient, logger, serializer =>
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
