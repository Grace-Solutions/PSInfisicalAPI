using System;
using System.Collections.Generic;
using System.Security;
using System.Text.RegularExpressions;
using PSInfisicalAPI.Authentication;
using PSInfisicalAPI.Logging;
using PSInfisicalAPI.Security;
using Xunit;

namespace PSInfisicalAPI.Tests
{
    public class InfisicalEnvironmentResolverTests : IDisposable
    {
        private const string TestPrefix = "PSINFITESTRESOLVER";
        private readonly List<string> _createdVariables = new List<string>();
        private readonly string _uniqueSuffix = Guid.NewGuid().ToString("N").ToUpperInvariant();

        private string SetProcessVar(string token, string value)
        {
            string name = TestPrefix + "_" + token + "_" + _uniqueSuffix;
            Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.Process);
            _createdVariables.Add(name);
            return name;
        }

        private Regex[] PatternsForThisTest(string tokenWithinName)
        {
            return new[]
            {
                new Regex("^" + TestPrefix + "_.*" + tokenWithinName + ".*" + _uniqueSuffix + "$", RegexOptions.IgnoreCase)
            };
        }

        public void Dispose()
        {
            for (int i = 0; i < _createdVariables.Count; i++)
            {
                Environment.SetEnvironmentVariable(_createdVariables[i], null, EnvironmentVariableTarget.Process);
            }
        }

        [Fact]
        public void Resolve_Returns_NotFound_When_No_Pattern_Matches()
        {
            Regex[] patterns = new[] { new Regex("WILL_NEVER_MATCH_" + Guid.NewGuid().ToString("N"), RegexOptions.IgnoreCase) };
            InfisicalEnvironmentResolver.ResolutionResult result = InfisicalEnvironmentResolver.Resolve(patterns);
            Assert.False(result.Found);
        }

        [Fact]
        public void Resolve_Returns_First_Matching_Variable_With_Value()
        {
            string name = SetProcessVar("CLIENTID", "client-1234");
            InfisicalEnvironmentResolver.ResolutionResult result = InfisicalEnvironmentResolver.Resolve(PatternsForThisTest("CLIENTID"));

            Assert.True(result.Found);
            Assert.Equal("client-1234", result.Value);
            Assert.Equal(name, result.VariableName);
            Assert.Equal(EnvironmentVariableTarget.Process, result.Scope);
        }

        [Fact]
        public void Resolve_Skips_Blank_Whitespace_Values()
        {
            SetProcessVar("CLIENTID_BLANK", "   ");
            string realName = SetProcessVar("CLIENTID_REAL", "client-xyz");

            Regex[] patterns = new[]
            {
                new Regex("^" + TestPrefix + "_CLIENTID_(BLANK|REAL)_" + _uniqueSuffix + "$", RegexOptions.IgnoreCase)
            };

            InfisicalEnvironmentResolver.ResolutionResult result = InfisicalEnvironmentResolver.Resolve(patterns);

            Assert.True(result.Found);
            Assert.Equal("client-xyz", result.Value);
            Assert.Equal(realName, result.VariableName);
        }

        [Fact]
        public void ResolveString_Returns_Current_Value_When_Already_Set()
        {
            SetProcessVar("PROJECTID_OVERRIDE", "env-project");
            string resolved = InfisicalEnvironmentResolver.ResolveString("ProjectId", PatternsForThisTest("PROJECTID"), "explicit-project", NullInfisicalLogger.Instance);
            Assert.Equal("explicit-project", resolved);
        }

        [Fact]
        public void ResolveString_Resolves_When_Current_Value_Is_Whitespace()
        {
            SetProcessVar("PROJECTID_FALLBACK", "env-project-123");
            string resolved = InfisicalEnvironmentResolver.ResolveString("ProjectId", PatternsForThisTest("PROJECTID"), "  ", NullInfisicalLogger.Instance);
            Assert.Equal("env-project-123", resolved);
        }

        [Fact]
        public void ResolveSecureString_Builds_ReadOnly_SecureString_From_Env()
        {
            SetProcessVar("ACCESSTOKEN_FOR_TEST", "tok-9999");
            SecureString resolved = InfisicalEnvironmentResolver.ResolveSecureString("AccessToken", PatternsForThisTest("ACCESSTOKEN"), null, NullInfisicalLogger.Instance);

            Assert.NotNull(resolved);
            Assert.True(resolved.IsReadOnly());
            string plain = SecureStringUtility.UsePlainText(resolved, p => p);
            Assert.Equal("tok-9999", plain);
        }

        [Fact]
        public void ResolveSecureString_Keeps_Existing_Populated_SecureString()
        {
            SetProcessVar("ACCESSTOKEN_IGNORE", "ignored");
            SecureString existing = SecureStringUtility.ToReadOnlySecureString("explicit-token");
            SecureString resolved = InfisicalEnvironmentResolver.ResolveSecureString("AccessToken", PatternsForThisTest("ACCESSTOKEN"), existing, NullInfisicalLogger.Instance);
            Assert.Same(existing, resolved);
        }
    }
}
