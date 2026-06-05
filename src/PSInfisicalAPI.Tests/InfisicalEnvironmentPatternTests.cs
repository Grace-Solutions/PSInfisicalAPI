using System.Text.RegularExpressions;
using PSInfisicalAPI.Authentication;
using Xunit;

namespace PSInfisicalAPI.Tests
{
    public class InfisicalEnvironmentPatternTests
    {
        [Theory]
        [InlineData("INFISICAL_API_URL")]
        [InlineData("INFISICAL_BASE_URL")]
        [InlineData("INFISICAL_BASE_URI")]
        [InlineData("INFISICAL_HOST")]
        [InlineData("CLOUDINIT_INFISICAL_APIURL")]
        public void BaseUriPatterns_Match_Expected_Names(string name)
        {
            Assert.True(MatchesAny(name, InfisicalEnvironmentResolver.BaseUriPatterns), "Expected match for " + name);
        }

        [Theory]
        [InlineData("INFISICAL_ORG_ID")]
        [InlineData("INFISICAL_ORGANIZATION_ID")]
        [InlineData("CLOUDINIT_INFISICAL_ORGANIZATIONID")]
        public void OrganizationIdPatterns_Match_Expected_Names(string name)
        {
            Assert.True(MatchesAny(name, InfisicalEnvironmentResolver.OrganizationIdPatterns), "Expected match for " + name);
        }

        [Theory]
        [InlineData("INFISICAL_CLIENT_ID")]
        [InlineData("INFISICAL_UNIVERSAL_AUTH_CLIENT_ID")]
        [InlineData("INFISICAL_MACHINE_IDENTITY_CLIENT_ID")]
        [InlineData("CLOUDINIT_INFISICAL_CLIENTID")]
        [InlineData("myapp_infisical_client_id")]
        public void ClientIdPatterns_Match_Standard_And_Custom_Prefixed_Names(string name)
        {
            Assert.True(MatchesAny(name, InfisicalEnvironmentResolver.ClientIdPatterns), "Expected match for " + name);
        }

        [Theory]
        [InlineData("INFISICAL_CLIENT_SECRET")]
        [InlineData("INFISICAL_UNIVERSAL_AUTH_CLIENT_SECRET")]
        [InlineData("INFISICAL_MACHINE_IDENTITY_CLIENT_SECRET")]
        [InlineData("CLOUDINIT_INFISICAL_CLIENTSECRET")]
        [InlineData("myapp_infisical_client_secret")]
        public void ClientSecretPatterns_Match_Standard_And_Custom_Prefixed_Names(string name)
        {
            Assert.True(MatchesAny(name, InfisicalEnvironmentResolver.ClientSecretPatterns), "Expected match for " + name);
        }

        [Theory]
        [InlineData("INFISICAL_TOKEN")]
        [InlineData("INFISICAL_ACCESS_TOKEN")]
        [InlineData("INFISICAL_AUTH_TOKEN")]
        [InlineData("CLOUDINIT_INFISICAL_TOKEN")]
        public void AccessTokenPatterns_Match_Expected_Names(string name)
        {
            Assert.True(MatchesAny(name, InfisicalEnvironmentResolver.AccessTokenPatterns), "Expected match for " + name);
        }

        [Theory]
        [InlineData("INFISICAL_SECRET_PATH")]
        [InlineData("INFISICAL_DEFAULT_SECRET_PATH")]
        [InlineData("CLOUDINIT_INFISICAL_SECRETPATH")]
        public void ClientSecretPatterns_Do_Not_Match_SecretPath_Variables(string name)
        {
            Assert.False(MatchesAny(name, InfisicalEnvironmentResolver.ClientSecretPatterns), "ClientSecretPatterns should NOT match " + name);
        }

        [Theory]
        [InlineData("PATH")]
        [InlineData("USERNAME")]
        [InlineData("HOME")]
        [InlineData("PROCESSOR_ARCHITECTURE")]
        public void Patterns_Do_Not_Match_Unrelated_System_Variables(string name)
        {
            Assert.False(MatchesAny(name, InfisicalEnvironmentResolver.ClientIdPatterns));
            Assert.False(MatchesAny(name, InfisicalEnvironmentResolver.ClientSecretPatterns));
            Assert.False(MatchesAny(name, InfisicalEnvironmentResolver.AccessTokenPatterns));
            Assert.False(MatchesAny(name, InfisicalEnvironmentResolver.BaseUriPatterns));
            Assert.False(MatchesAny(name, InfisicalEnvironmentResolver.OrganizationIdPatterns));
            Assert.False(MatchesAny(name, InfisicalEnvironmentResolver.ApiVersionPatterns));
        }

        private static bool MatchesAny(string input, Regex[] patterns)
        {
            for (int i = 0; i < patterns.Length; i++)
            {
                if (patterns[i].IsMatch(input))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
