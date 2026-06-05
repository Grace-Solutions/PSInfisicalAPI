using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Reflection;
using PSInfisicalAPI.Cmdlets;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Logging;
using Xunit;

namespace PSInfisicalAPI.Tests
{
    public class CmdletBaseInheritanceTests
    {
        private sealed class RecordingLogger : IInfisicalLogger
        {
            public List<string> VerboseEntries { get; } = new List<string>();

            public void Information(string component, string message) { }
            public void Verbose(string component, string message) { VerboseEntries.Add(message); }
            public void Debug(string component, string message) { }
            public void Warning(string component, string message) { }
            public void Error(string component, string message) { }
        }

        [Cmdlet(VerbsCommon.Get, "TestCmdlet")]
        private sealed class TestCmdlet : InfisicalCmdletBase
        {
            public string CallResolveApiVersion(InfisicalConnection connection, string explicitValue)
            {
                return ResolveApiVersion(connection, explicitValue);
            }

            public string CallResolveOrganizationId(InfisicalConnection connection, string explicitValue)
            {
                return ResolveOrganizationId(connection, explicitValue);
            }
        }

        private static TestCmdlet CreateCmdletWith(RecordingLogger logger)
        {
            TestCmdlet cmdlet = new TestCmdlet();
            FieldInfo field = typeof(InfisicalCmdletBase).GetField("_logger", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(cmdlet, logger);
            return cmdlet;
        }

        private static InfisicalConnection ConnectionWithDefaults()
        {
            return new InfisicalConnection
            {
                BaseUri = new Uri("https://app.example.com"),
                OrganizationId = "org-conn",
                PinnedApiVersion = "v3"
            };
        }

        [Fact]
        public void ResolveApiVersion_Prefers_PinnedApiVersion_From_Connection()
        {
            RecordingLogger logger = new RecordingLogger();
            TestCmdlet cmdlet = CreateCmdletWith(logger);

            string resolved = cmdlet.CallResolveApiVersion(ConnectionWithDefaults(), null);
            Assert.Equal("v3", resolved);
        }

        [Fact]
        public void ResolveOrganizationId_Inherits_From_Connection_And_Logs()
        {
            RecordingLogger logger = new RecordingLogger();
            TestCmdlet cmdlet = CreateCmdletWith(logger);

            Assert.Equal("org-conn", cmdlet.CallResolveOrganizationId(ConnectionWithDefaults(), null));
            Assert.Single(logger.VerboseEntries);
            Assert.Contains("OrganizationId", logger.VerboseEntries[0]);
        }

        [Fact]
        public void ResolveOrganizationId_Explicit_Value_Wins_And_Does_Not_Log()
        {
            RecordingLogger logger = new RecordingLogger();
            TestCmdlet cmdlet = CreateCmdletWith(logger);

            Assert.Equal("explicit-org", cmdlet.CallResolveOrganizationId(ConnectionWithDefaults(), "explicit-org"));
            Assert.Empty(logger.VerboseEntries);
        }

        [Fact]
        public void InfisicalConnection_Defaults_TransportFlags_To_False()
        {
            InfisicalConnection connection = new InfisicalConnection();
            Assert.False(connection.SkipCertificateCheck);
            Assert.False(connection.AllowInsecureTransport);
        }

        [Fact]
        public void ShouldSkipCertificateCheck_Reads_From_Current_Session()
        {
            InfisicalConnection previous = InfisicalSessionManager.Current;
            try
            {
                TestCmdlet cmdlet = CreateCmdletWith(new RecordingLogger());
                MethodInfo virt = typeof(InfisicalCmdletBase).GetMethod("ShouldSkipCertificateCheck", BindingFlags.NonPublic | BindingFlags.Instance);

                InfisicalSessionManager.SetCurrent(null);
                Assert.False((bool)virt.Invoke(cmdlet, null));

                InfisicalConnection session = ConnectionWithDefaults();
                session.IsConnected = true;
                session.SkipCertificateCheck = true;
                InfisicalSessionManager.SetCurrent(session);

                Assert.True((bool)virt.Invoke(cmdlet, null));
            }
            finally
            {
                InfisicalSessionManager.SetCurrent(previous);
            }
        }
    }
}
