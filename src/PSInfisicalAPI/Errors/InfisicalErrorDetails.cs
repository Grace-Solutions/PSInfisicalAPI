namespace PSInfisicalAPI.Errors
{
    public sealed class InfisicalErrorDetails
    {
        public string Component { get; set; }
        public string Operation { get; set; }
        public string Message { get; set; }
        public string ExceptionType { get; set; }
        public string InnerExceptionMessage { get; set; }
        public int? StatusCode { get; set; }
        public string ReasonPhrase { get; set; }
        public string ApiErrorCode { get; set; }
        public string SanitizedBody { get; set; }
        public int? LineNumber { get; set; }
        public int? LinePosition { get; set; }
        public string EndpointName { get; set; }
        public string RequestMethod { get; set; }
    }
}
