using System;
using System.Management.Automation;
using PSInfisicalAPI.Errors;
using PSInfisicalAPI.Http;
using PSInfisicalAPI.Logging;

namespace PSInfisicalAPI.Cmdlets
{
    public abstract class InfisicalCmdletBase : PSCmdlet
    {
        private IInfisicalLogger _logger;
        private IInfisicalHttpClient _httpClient;

        protected IInfisicalLogger Logger
        {
            get
            {
                if (_logger == null)
                {
                    _logger = new PSCmdletLogger(this);
                }

                return _logger;
            }
        }

        protected IInfisicalHttpClient HttpClient
        {
            get
            {
                if (_httpClient == null)
                {
                    _httpClient = new InfisicalHttpClient(Logger);
                }

                return _httpClient;
            }
        }

        protected void ThrowTerminatingForException(string component, string operation, Exception exception)
        {
            InfisicalErrorDetails details = InfisicalErrorHandler.BuildDetails(component, operation, exception);
            InfisicalErrorHandler.LogFailure(Logger, details);
            ErrorRecord record = InfisicalErrorHandler.ToErrorRecord(exception, details);
            ThrowTerminatingError(record);
        }
    }
}
