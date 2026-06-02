using System;
using System.Management.Automation;

namespace PSInfisicalAPI.Logging
{
    public sealed class PSCmdletLogger : IInfisicalLogger
    {
        private readonly Cmdlet _cmdlet;

        public PSCmdletLogger(Cmdlet cmdlet)
        {
            _cmdlet = cmdlet ?? throw new ArgumentNullException(nameof(cmdlet));
        }

        public void Information(string component, string message)
        {
            string line = InfisicalLogFormatter.FormatNow(InfisicalLogLevel.Information, component, message);
            _cmdlet.WriteVerbose(line);
        }

        public void Verbose(string component, string message)
        {
            string line = InfisicalLogFormatter.FormatNow(InfisicalLogLevel.Verbose, component, message);
            _cmdlet.WriteVerbose(line);
        }

        public void Debug(string component, string message)
        {
            string line = InfisicalLogFormatter.FormatNow(InfisicalLogLevel.Debug, component, message);
            _cmdlet.WriteDebug(line);
        }

        public void Warning(string component, string message)
        {
            string line = InfisicalLogFormatter.FormatNow(InfisicalLogLevel.Warning, component, message);
            _cmdlet.WriteWarning(line);
        }

        public void Error(string component, string message)
        {
            string line = InfisicalLogFormatter.FormatNow(InfisicalLogLevel.Error, component, message);
            ErrorRecord record = new ErrorRecord(
                new InvalidOperationException(message ?? string.Empty),
                "PSInfisicalAPI.Error",
                ErrorCategory.NotSpecified,
                component);
            record.ErrorDetails = new ErrorDetails(line);
            _cmdlet.WriteError(record);
        }
    }
}
