using PSInfisicalAPI.Errors;
using Xunit;

namespace PSInfisicalAPI.Tests
{
    public class ErrorHandlerTests
    {
        [Fact]
        public void BuildDetails_Preserves_Http_Status_Code()
        {
            InfisicalApiException exception = new InfisicalApiException("Forbidden")
            {
                StatusCode = 403,
                ReasonPhrase = "Forbidden",
                EndpointName = "ListSecrets",
                RequestMethod = "GET",
                ApiErrorCode = "PERMISSION_DENIED",
                SanitizedBody = "[REDACTED]"
            };

            InfisicalErrorDetails details = InfisicalErrorHandler.BuildDetails("SecretsClient", "RetrieveSecrets", exception);

            Assert.Equal(403, details.StatusCode);
            Assert.Equal("Forbidden", details.ReasonPhrase);
            Assert.Equal("PERMISSION_DENIED", details.ApiErrorCode);
            Assert.Equal("ListSecrets", details.EndpointName);
            Assert.Equal("[REDACTED]", details.SanitizedBody);
            Assert.Equal("SecretsClient", details.Component);
            Assert.Equal("RetrieveSecrets", details.Operation);
        }

        [Fact]
        public void BuildDetails_Preserves_Serialization_Line_Information()
        {
            InfisicalSerializationException exception = new InfisicalSerializationException("Bad JSON")
            {
                LineNumber = 12,
                LinePosition = 34
            };

            InfisicalErrorDetails details = InfisicalErrorHandler.BuildDetails("Serializer", "Deserialize", exception);

            Assert.Equal(12, details.LineNumber);
            Assert.Equal(34, details.LinePosition);
        }

        [Fact]
        public void MapCategory_Maps_Auth_Exception()
        {
            System.Management.Automation.ErrorCategory category = InfisicalErrorHandler.MapCategory(new InfisicalAuthenticationException("Bad creds"));
            Assert.Equal(System.Management.Automation.ErrorCategory.AuthenticationError, category);
        }

        [Fact]
        public void MapCategory_Maps_Api_Exception_By_Status_Code()
        {
            Assert.Equal(System.Management.Automation.ErrorCategory.AuthenticationError, InfisicalErrorHandler.MapCategory(new InfisicalApiException("x") { StatusCode = 401 }));
            Assert.Equal(System.Management.Automation.ErrorCategory.PermissionDenied, InfisicalErrorHandler.MapCategory(new InfisicalApiException("x") { StatusCode = 403 }));
            Assert.Equal(System.Management.Automation.ErrorCategory.ObjectNotFound, InfisicalErrorHandler.MapCategory(new InfisicalApiException("x") { StatusCode = 404 }));
            Assert.Equal(System.Management.Automation.ErrorCategory.ResourceUnavailable, InfisicalErrorHandler.MapCategory(new InfisicalApiException("x") { StatusCode = 503 }));
        }
    }
}
