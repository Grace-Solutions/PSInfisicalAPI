using System;
using System.Globalization;
using System.Management.Automation;
using PSInfisicalAPI.Logging;

namespace PSInfisicalAPI.Errors
{
    public static class InfisicalErrorHandler
    {
        private const string Component = "ErrorHandler";

        public static InfisicalErrorDetails BuildDetails(string component, string operation, Exception exception)
        {
            InfisicalErrorDetails details = new InfisicalErrorDetails
            {
                Component = component,
                Operation = operation,
                Message = exception?.Message,
                ExceptionType = exception?.GetType().FullName,
                InnerExceptionMessage = exception?.InnerException?.Message
            };

            InfisicalApiException apiException = exception as InfisicalApiException;
            if (apiException != null)
            {
                details.StatusCode = apiException.StatusCode;
                details.ReasonPhrase = apiException.ReasonPhrase;
                details.ApiErrorCode = apiException.ApiErrorCode;
                details.SanitizedBody = apiException.SanitizedBody;
                details.EndpointName = apiException.EndpointName;
                details.RequestMethod = apiException.RequestMethod;
            }

            InfisicalSerializationException serializationException = exception as InfisicalSerializationException;
            if (serializationException != null)
            {
                details.LineNumber = serializationException.LineNumber;
                details.LinePosition = serializationException.LinePosition;
            }

            return details;
        }

        public static void LogFailure(IInfisicalLogger logger, InfisicalErrorDetails details)
        {
            if (logger == null || details == null)
            {
                return;
            }

            logger.Error(Component, string.Concat("Operation failed: ", details.Operation ?? "Unspecified"));

            if (!string.IsNullOrEmpty(details.Component))
            {
                logger.Error(Component, string.Concat("Error Component: ", details.Component));
            }

            if (!string.IsNullOrEmpty(details.Message))
            {
                logger.Error(Component, string.Concat("Error Message: ", details.Message));
            }

            if (details.StatusCode.HasValue)
            {
                logger.Error(Component, string.Concat("HTTP Status Code: ", details.StatusCode.Value.ToString(CultureInfo.InvariantCulture)));
            }

            if (!string.IsNullOrEmpty(details.ApiErrorCode))
            {
                logger.Error(Component, string.Concat("API Error Code: ", details.ApiErrorCode));
            }

            if (details.LineNumber.HasValue)
            {
                logger.Error(Component, string.Concat("Line: ", details.LineNumber.Value.ToString(CultureInfo.InvariantCulture)));
            }

            if (details.LinePosition.HasValue)
            {
                logger.Error(Component, string.Concat("Position: ", details.LinePosition.Value.ToString(CultureInfo.InvariantCulture)));
            }
        }

        public static ErrorRecord ToErrorRecord(Exception exception, InfisicalErrorDetails details)
        {
            ErrorCategory category = MapCategory(exception);
            string errorId = exception?.GetType().Name ?? "PSInfisicalAPI.Error";
            object target = details?.Component;

            ErrorRecord record = new ErrorRecord(exception ?? new InfisicalException("Unspecified error."), errorId, category, target);

            if (details != null && !string.IsNullOrEmpty(details.Message))
            {
                record.ErrorDetails = new ErrorDetails(details.Message);
            }

            return record;
        }

        internal static ErrorCategory MapCategory(Exception exception)
        {
            if (exception is InfisicalAuthenticationException) { return ErrorCategory.AuthenticationError; }
            if (exception is InfisicalHttpException) { return ErrorCategory.ConnectionError; }
            if (exception is InfisicalSerializationException) { return ErrorCategory.InvalidData; }
            if (exception is InfisicalConfigurationException) { return ErrorCategory.InvalidArgument; }
            if (exception is InfisicalExportException) { return ErrorCategory.WriteError; }

            InfisicalApiException apiException = exception as InfisicalApiException;
            if (apiException != null)
            {
                if (apiException.StatusCode == 401) { return ErrorCategory.AuthenticationError; }
                if (apiException.StatusCode == 403) { return ErrorCategory.PermissionDenied; }
                if (apiException.StatusCode == 404) { return ErrorCategory.ObjectNotFound; }
                if (apiException.StatusCode == 503) { return ErrorCategory.ResourceUnavailable; }

                return ErrorCategory.InvalidOperation;
            }

            return ErrorCategory.NotSpecified;
        }
    }
}
