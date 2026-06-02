namespace PSInfisicalAPI.Logging
{
    public sealed class NullInfisicalLogger : IInfisicalLogger
    {
        public static readonly NullInfisicalLogger Instance = new NullInfisicalLogger();

        private NullInfisicalLogger() { }

        public void Information(string component, string message) { }
        public void Verbose(string component, string message) { }
        public void Debug(string component, string message) { }
        public void Warning(string component, string message) { }
        public void Error(string component, string message) { }
    }
}
