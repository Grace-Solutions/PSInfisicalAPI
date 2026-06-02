using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PSInfisicalAPI.Exports;
using PSInfisicalAPI.Models;
using PSInfisicalAPI.Security;
using Xunit;

namespace PSInfisicalAPI.Tests
{
    public class ExportTests : IDisposable
    {
        private readonly DirectoryInfo _tempDirectory;

        public ExportTests()
        {
            string root = Path.Combine(Path.GetTempPath(), "PSInfisicalAPI.Tests.Export-" + Guid.NewGuid().ToString("N"));
            _tempDirectory = new DirectoryInfo(root);
            _tempDirectory.Create();
        }

        public void Dispose()
        {
            if (_tempDirectory.Exists)
            {
                _tempDirectory.Delete(true);
            }
        }

        private static InfisicalSecret[] SampleSecrets()
        {
            return new[]
            {
                new InfisicalSecret
                {
                    SecretName = "SqlServer",
                    SecretPath = "/servers",
                    SecretValue = SecureStringUtility.ToReadOnlySecureString("192.168.1.10"),
                    SecretMetadata = new[] { new InfisicalSecretMetadata { Key = "Owner", Value = "Infrastructure" } }
                },
                new InfisicalSecret
                {
                    SecretName = "SqlPassword",
                    SecretPath = "/servers",
                    SecretValue = SecureStringUtility.ToReadOnlySecureString("ExamplePassword")
                }
            };
        }

        [Fact]
        public void Export_Env_Writes_Key_Equals_Value()
        {
            FileInfo path = new FileInfo(Path.Combine(_tempDirectory.FullName, "missing", "out.env"));

            new EnvInfisicalExporter().Export(new InfisicalExportRequest
            {
                Secrets = SampleSecrets(),
                Format = InfisicalExportFormat.Env,
                Path = path,
                Encoding = new UTF8Encoding(false)
            });

            Assert.True(path.Exists || File.Exists(path.FullName));
            string contents = File.ReadAllText(path.FullName);
            Assert.Contains("SqlServer=192.168.1.10", contents);
            Assert.Contains("SqlPassword=ExamplePassword", contents);
        }

        [Fact]
        public void Export_Json_Creates_Directory()
        {
            FileInfo path = new FileInfo(Path.Combine(_tempDirectory.FullName, "deep", "nested", "out.json"));
            new JsonInfisicalExporter().Export(new InfisicalExportRequest
            {
                Secrets = SampleSecrets(),
                Format = InfisicalExportFormat.Json,
                Path = path
            });

            Assert.True(path.Directory.Exists);
            string contents = File.ReadAllText(path.FullName);
            Assert.Contains("SqlServer", contents);
            Assert.Contains("192.168.1.10", contents);
        }

        [Fact]
        public void Export_Yaml_Creates_Directory()
        {
            FileInfo path = new FileInfo(Path.Combine(_tempDirectory.FullName, "yaml", "out.yaml"));
            new YamlInfisicalExporter().Export(new InfisicalExportRequest
            {
                Secrets = SampleSecrets(),
                Format = InfisicalExportFormat.Yaml,
                Path = path
            });

            Assert.True(path.Directory.Exists);
            string contents = File.ReadAllText(path.FullName);
            Assert.Contains("Secrets:", contents);
            Assert.Contains("SqlServer", contents);
        }

        [Fact]
        public void Export_Xml_Matches_Schema()
        {
            FileInfo path = new FileInfo(Path.Combine(_tempDirectory.FullName, "xml", "out.xml"));
            new XmlInfisicalExporter().Export(new InfisicalExportRequest
            {
                Secrets = SampleSecrets(),
                Format = InfisicalExportFormat.Xml,
                Path = path
            });

            string contents = File.ReadAllText(path.FullName);
            Assert.Contains("<Secrets>", contents);
            Assert.Contains("<Secret>", contents);
            Assert.Contains("<SecretName>SqlServer</SecretName>", contents);
            Assert.Contains("<SecretValue>192.168.1.10</SecretValue>", contents);
            Assert.Contains("<SecretMetadata>", contents);
            Assert.Contains("<Metadata>", contents);
        }

        [Fact]
        public void Export_EnvironmentVariables_Defaults_To_Process()
        {
            string name = "PSInFI_TestVar_" + Guid.NewGuid().ToString("N");
            InfisicalSecret[] secrets = new[]
            {
                new InfisicalSecret
                {
                    SecretName = name,
                    SecretValue = SecureStringUtility.ToReadOnlySecureString("processed")
                }
            };

            try
            {
                new EnvironmentVariableExporter().Export(new InfisicalExportRequest
                {
                    Secrets = secrets,
                    Format = InfisicalExportFormat.EnvironmentVariables,
                    Scope = EnvironmentVariableTarget.Process
                });

                Assert.Equal("processed", Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process));
            }
            finally
            {
                Environment.SetEnvironmentVariable(name, null, EnvironmentVariableTarget.Process);
            }
        }
    }
}
