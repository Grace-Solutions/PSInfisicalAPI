using System;

namespace PSInfisicalAPI.Errors
{
    public class InfisicalException : Exception
    {
        public string Component { get; set; }
        public string Operation { get; set; }

        public InfisicalException() { }

        public InfisicalException(string message) : base(message) { }

        public InfisicalException(string message, Exception innerException) : base(message, innerException) { }

        public InfisicalException(string component, string operation, string message)
            : base(message)
        {
            Component = component;
            Operation = operation;
        }

        public InfisicalException(string component, string operation, string message, Exception innerException)
            : base(message, innerException)
        {
            Component = component;
            Operation = operation;
        }
    }

    public class InfisicalApiException : InfisicalException
    {
        public int StatusCode { get; set; }
        public string ReasonPhrase { get; set; }
        public string ApiErrorCode { get; set; }
        public string ApiErrorMessage { get; set; }
        public string ApiRequestId { get; set; }
        public string SanitizedBody { get; set; }
        public string EndpointName { get; set; }
        public string RequestMethod { get; set; }

        public InfisicalApiException() { }
        public InfisicalApiException(string message) : base(message) { }
        public InfisicalApiException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class InfisicalAuthenticationException : InfisicalException
    {
        public InfisicalAuthenticationException() { }
        public InfisicalAuthenticationException(string message) : base(message) { }
        public InfisicalAuthenticationException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class InfisicalHttpException : InfisicalException
    {
        public InfisicalHttpException() { }
        public InfisicalHttpException(string message) : base(message) { }
        public InfisicalHttpException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class InfisicalSerializationException : InfisicalException
    {
        public int? LineNumber { get; set; }
        public int? LinePosition { get; set; }

        public InfisicalSerializationException() { }
        public InfisicalSerializationException(string message) : base(message) { }
        public InfisicalSerializationException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class InfisicalExportException : InfisicalException
    {
        public InfisicalExportException() { }
        public InfisicalExportException(string message) : base(message) { }
        public InfisicalExportException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class InfisicalImportException : InfisicalException
    {
        public InfisicalImportException() { }
        public InfisicalImportException(string message) : base(message) { }
        public InfisicalImportException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class InfisicalConfigurationException : InfisicalException
    {
        public InfisicalConfigurationException() { }
        public InfisicalConfigurationException(string message) : base(message) { }
        public InfisicalConfigurationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
