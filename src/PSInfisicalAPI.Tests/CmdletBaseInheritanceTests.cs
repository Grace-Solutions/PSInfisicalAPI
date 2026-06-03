using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Reflection;
using PSInfisicalAPI.Cmdlets;
using PSInfisicalAPI.Connections;
using PSInfisicalAPI.Logging;
using PSInfisicalAPI.Models;
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
            public string CallResolveProjectId(InfisicalConnection connection, string explicitValue)
            {
                return ResolveProjectId(connection, explicitValue);
            }

            public string CallResolveEnvironment(InfisicalConnection connection, string explicitValue)
            {
                return ResolveEnvironment(connection, explicitValue);
            }

            public string CallResolveSecretPath(InfisicalConnection connection, string explicitValue)
            {
                return ResolveSecretPath(connection, explicitValue);
            }

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
                ProjectId = "proj-conn",
                Environment = "prod-conn",
                DefaultSecretPath = "/db",
                OrganizationId = "org-conn",
                PinnedApiVersion = "v3"
            };
        }

        [Fact]
        public void Explicit_Value_Overrides_Connection_And_Does_Not_Log()
        {
            RecordingLogger logger = new RecordingLogger();
            TestCmdlet cmdlet = CreateCmdletWith(logger);

            string resolved = cmdlet.CallResolveProjectId(ConnectionWithDefaults(), "explicit-proj");
            Assert.Equal("explicit-proj", resolved);
            Assert.Empty(logger.VerboseEntries);
        }

        [Fact]
        public void Missing_Value_Inherits_From_Connection_And_Logs()
        {
            RecordingLogger logger = new RecordingLogger();
            TestCmdlet cmdlet = CreateCmdletWith(logger);

            string resolved = cmdlet.CallResolveProjectId(ConnectionWithDefaults(), null);
            Assert.Equal("proj-conn", resolved);
            Assert.Single(logger.VerboseEntries);
            Assert.Contains("Inherited ProjectId", logger.VerboseEntries[0]);
            Assert.Contains("proj-conn", logger.VerboseEntries[0]);
        }

        [Fact]
        public void ResolveSecretPath_Defaults_To_Root_When_Connection_Has_No_Default()
        {
            RecordingLogger logger = new RecordingLogger();
            TestCmdlet cmdlet = CreateCmdletWith(logger);

            InfisicalConnection bareConnection = new InfisicalConnection { BaseUri = new Uri("https://app.example.com") };
            string resolved = cmdlet.CallResolveSecretPath(bareConnection, null);
            Assert.Equal("/", resolved);
        }

        [Fact]
        public void ResolveSecretPath_Inherits_From_Connection_When_Set()
        {
            RecordingLogger logger = new RecordingLogger();
            TestCmdlet cmdlet = CreateCmdletWith(logger);

            string resolved = cmdlet.CallResolveSecretPath(ConnectionWithDefaults(), null);
            Assert.Equal("/db", resolved);
            Assert.Contains(logger.VerboseEntries, v => v.Contains("SecretPath") && v.Contains("/db"));
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
        public void ResolveEnvironment_And_ResolveOrganizationId_Inherit()
        {
            RecordingLogger logger = new RecordingLogger();
            TestCmdlet cmdlet = CreateCmdletWith(logger);

            Assert.Equal("prod-conn", cmdlet.CallResolveEnvironment(ConnectionWithDefaults(), null));
            Assert.Equal("org-conn", cmdlet.CallResolveOrganizationId(ConnectionWithDefaults(), null));
            Assert.Equal(2, logger.VerboseEntries.Count);
        }
    }
}
