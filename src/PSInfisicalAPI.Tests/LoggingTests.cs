using System;
using System.Text.RegularExpressions;
using PSInfisicalAPI.Logging;
using Xunit;

namespace PSInfisicalAPI.Tests
{
    public class LoggingTests
    {
        [Fact]
        public void Format_Uses_Utc_Timestamp_And_Component()
        {
            DateTimeOffset utc = new DateTimeOffset(2026, 6, 2, 21, 44, 22, TimeSpan.Zero).AddTicks(1830000);
            string result = InfisicalLogFormatter.Format(utc, InfisicalLogLevel.Information, "SecretsClient", "Attempting to retrieve Infisical secrets. Please Wait...");

            Assert.Equal("[2026-06-02T21:44:22.1830000Z] - [Information] - [SecretsClient] - Attempting to retrieve Infisical secrets. Please Wait...", result);
        }

        [Fact]
        public void Format_Includes_All_Levels()
        {
            DateTimeOffset utc = DateTimeOffset.UtcNow;
            foreach (InfisicalLogLevel level in Enum.GetValues(typeof(InfisicalLogLevel)))
            {
                string result = InfisicalLogFormatter.Format(utc, level, "Component", "Message");
                Assert.Matches(@"^\[[0-9TZ:\.\-]+\] - \[" + level + @"\] - \[Component\] - Message$", result);
            }
        }

        [Fact]
        public void NullLogger_Accepts_Any_Calls()
        {
            IInfisicalLogger logger = NullInfisicalLogger.Instance;
            logger.Information("c", "m");
            logger.Verbose("c", "m");
            logger.Debug("c", "m");
            logger.Warning("c", "m");
            logger.Error("c", "m");
        }
    }
}
