using System.Collections.Generic;
using PSInfisicalAPI.Common;
using Xunit;

namespace PSInfisicalAPI.Tests
{
    public class SanitizerTests
    {
        [Fact]
        public void Sanitize_Body_Redacts_When_Contains_Secrets()
        {
            string body = "{\"secretValue\":\"abc\"}";
            string sanitized = InfisicalSanitizer.SanitizeBody(body, true);
            Assert.Equal("[REDACTED]", sanitized);
        }

        [Fact]
        public void Sanitize_Body_Truncates_Long_NonSecret_Body()
        {
            string body = new string('a', 4096);
            string sanitized = InfisicalSanitizer.SanitizeBody(body, false);
            Assert.Contains("[truncated]", sanitized);
        }

        [Fact]
        public void Sanitize_Header_Redacts_Authorization()
        {
            string sanitized = InfisicalSanitizer.SanitizeHeaderValue("Authorization", "Bearer abc.def");
            Assert.Equal("[REDACTED]", sanitized);
        }

        [Fact]
        public void Sanitize_Header_Passes_Through_Non_Sensitive()
        {
            string sanitized = InfisicalSanitizer.SanitizeHeaderValue("Accept", "application/json");
            Assert.Equal("application/json", sanitized);
        }

        [Fact]
        public void Sanitize_Headers_Returns_New_Map()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer token" },
                { "Accept", "application/json" }
            };

            IDictionary<string, string> sanitized = InfisicalSanitizer.SanitizeHeaders(headers);
            Assert.Equal("[REDACTED]", sanitized["Authorization"]);
            Assert.Equal("application/json", sanitized["Accept"]);
        }
    }
}
