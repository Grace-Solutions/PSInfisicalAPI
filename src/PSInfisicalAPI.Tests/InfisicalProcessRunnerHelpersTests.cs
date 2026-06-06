using System;
using PSInfisicalAPI.Process;
using Xunit;

namespace PSInfisicalAPI.Tests
{
    public class InfisicalProcessRunnerHelpersTests
    {
        [Fact]
        public void FormatFriendly_Zero_Returns_NotAvailable()
        {
            Assert.Equal("N/A", InfisicalProcessRunnerHelpers.FormatFriendly(TimeSpan.Zero));
        }

        [Fact]
        public void FormatFriendly_Single_Unit_Plural()
        {
            Assert.Equal("30 seconds", InfisicalProcessRunnerHelpers.FormatFriendly(TimeSpan.FromSeconds(30)));
            Assert.Equal("5 minutes", InfisicalProcessRunnerHelpers.FormatFriendly(TimeSpan.FromMinutes(5)));
            Assert.Equal("250 milliseconds", InfisicalProcessRunnerHelpers.FormatFriendly(TimeSpan.FromMilliseconds(250)));
        }

        [Fact]
        public void FormatFriendly_Single_Unit_Singular()
        {
            Assert.Equal("1 second", InfisicalProcessRunnerHelpers.FormatFriendly(TimeSpan.FromSeconds(1)));
            Assert.Equal("1 minute", InfisicalProcessRunnerHelpers.FormatFriendly(TimeSpan.FromMinutes(1)));
            Assert.Equal("1 hour", InfisicalProcessRunnerHelpers.FormatFriendly(TimeSpan.FromHours(1)));
            Assert.Equal("1 day", InfisicalProcessRunnerHelpers.FormatFriendly(TimeSpan.FromDays(1)));
            Assert.Equal("1 millisecond", InfisicalProcessRunnerHelpers.FormatFriendly(TimeSpan.FromMilliseconds(1)));
        }

        [Fact]
        public void FormatFriendly_Two_Units_Uses_And_Join()
        {
            TimeSpan value = TimeSpan.FromSeconds(7) + TimeSpan.FromMilliseconds(364);
            Assert.Equal("7 seconds, and 364 milliseconds", InfisicalProcessRunnerHelpers.FormatFriendly(value));
        }

        [Fact]
        public void FormatFriendly_Multiple_Units_Uses_Comma_And_Trailing_And()
        {
            TimeSpan value = TimeSpan.FromHours(1) + TimeSpan.FromMinutes(2) + TimeSpan.FromSeconds(3) + TimeSpan.FromMilliseconds(45);
            Assert.Equal("1 hour, 2 minutes, 3 seconds, and 45 milliseconds", InfisicalProcessRunnerHelpers.FormatFriendly(value));
        }

        [Fact]
        public void FormatFriendly_Skips_Zero_Components()
        {
            TimeSpan value = TimeSpan.FromHours(2) + TimeSpan.FromMilliseconds(500);
            Assert.Equal("2 hours, and 500 milliseconds", InfisicalProcessRunnerHelpers.FormatFriendly(value));
        }

        [Fact]
        public void FormatFriendly_Mixed_Singular_And_Plural()
        {
            TimeSpan value = TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(30);
            Assert.Equal("1 minute, and 30 seconds", InfisicalProcessRunnerHelpers.FormatFriendly(value));
        }

        [Fact]
        public void FormatFriendly_Days_Component()
        {
            TimeSpan value = TimeSpan.FromDays(2) + TimeSpan.FromHours(3);
            Assert.Equal("2 days, and 3 hours", InfisicalProcessRunnerHelpers.FormatFriendly(value));
        }

        [Fact]
        public void FormatFriendly_SubMillisecond_Returns_NotAvailable()
        {
            TimeSpan value = TimeSpan.FromTicks(100);
            Assert.Equal("N/A", InfisicalProcessRunnerHelpers.FormatFriendly(value));
        }
    }
}
