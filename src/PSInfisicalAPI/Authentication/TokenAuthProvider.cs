using PSInfisicalAPI.Errors;
using PSInfisicalAPI.Http;
using PSInfisicalAPI.Logging;

namespace PSInfisicalAPI.Authentication
{
    public sealed class TokenAuthProvider : IInfisicalAuthProvider
    {
        public string Name { get { return "Token"; } }

        public InfisicalAuthenticationResult Authenticate(InfisicalAuthenticationRequest request, IInfisicalHttpClient httpClient, IInfisicalLogger logger)
        {
            if (request == null || request.PreSuppliedAccessToken == null)
            {
                throw new InfisicalAuthenticationException("An access token must be supplied for Token authentication.");
            }

            return new InfisicalAuthenticationResult
            {
                AccessToken = request.PreSuppliedAccessToken,
                TokenType = "Bearer"
            };
        }
    }
}
