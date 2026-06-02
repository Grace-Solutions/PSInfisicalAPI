namespace PSInfisicalAPI.Http
{
    public interface IInfisicalHttpClient
    {
        InfisicalHttpResponse Send(InfisicalHttpRequest request);
    }
}
