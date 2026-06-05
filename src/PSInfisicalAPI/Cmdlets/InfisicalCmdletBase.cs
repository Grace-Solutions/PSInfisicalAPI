using System;
using System.Management.Automation;
using PSInfisicalAPI.Connections;
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

        protected string ResolveApiVersion(InfisicalConnection connection, string explicitValue)
        {
            string fromConnection = connection != null ? (!string.IsNullOrEmpty(connection.PinnedApiVersion) ? connection.PinnedApiVersion : connection.ApiVersion) : null;
            return ResolveValue("ApiVersion", explicitValue, fromConnection, null);
        }

        protected string ResolveOrganizationId(InfisicalConnection connection, string explicitValue)
        {
            return ResolveValue("OrganizationId", explicitValue, connection != null ? connection.OrganizationId : null, null);
        }

        private string ResolveValue(string parameterName, string explicitValue, string inheritedValue, string defaultValue)
        {
            if (!string.IsNullOrEmpty(explicitValue)) { return explicitValue; }
            if (!string.IsNullOrEmpty(inheritedValue))
            {
                Logger.Verbose(GetType().Name, string.Concat("Inherited ", parameterName, " '", inheritedValue, "' from connection."));
                return inheritedValue;
            }

            return defaultValue;
        }
    }
}
