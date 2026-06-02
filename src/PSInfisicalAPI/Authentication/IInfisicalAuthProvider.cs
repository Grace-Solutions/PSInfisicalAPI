using PSInfisicalAPI.Http;
using PSInfisicalAPI.Logging;

namespace PSInfisicalAPI.Authentication
{
    public interface IInfisicalAuthProvider
    {
        string Name { get; }

        InfisicalAuthenticationResult Authenticate(InfisicalAuthenticationRequest request, IInfisicalHttpClient httpClient, IInfisicalLogger logger);
    }
}
