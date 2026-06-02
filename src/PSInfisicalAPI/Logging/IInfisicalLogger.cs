namespace PSInfisicalAPI.Logging
{
    public interface IInfisicalLogger
    {
        void Information(string component, string message);
        void Verbose(string component, string message);
        void Debug(string component, string message);
        void Warning(string component, string message);
        void Error(string component, string message);
    }
}
