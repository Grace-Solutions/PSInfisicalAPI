using PSInfisicalAPI.Endpoints;
using PSInfisicalAPI.Errors;
using Xunit;

namespace PSInfisicalAPI.Tests
{
    public class EndpointRegistryTests
    {
        [Fact]
        public void Get_ListSecrets_Returns_Definition()
        {
            InfisicalEndpointDefinition definition = InfisicalEndpointRegistry.Get(InfisicalEndpointNames.ListSecrets);
            Assert.Equal("GET", definition.Method);
            Assert.Equal("v4", definition.Version);
            Assert.Equal("/api/v4/secrets", definition.Template);
            Assert.True(definition.RequiresAuthorization);
            Assert.False(definition.ContainsSecretMaterialInRequest);
            Assert.True(definition.ContainsSecretMaterialInResponse);
        }

        [Fact]
        public void Get_RetrieveSecret_Returns_Definition()
        {
            InfisicalEndpointDefinition definition = InfisicalEndpointRegistry.Get(InfisicalEndpointNames.RetrieveSecret);
            Assert.Equal("GET", definition.Method);
            Assert.Equal("v4", definition.Version);
            Assert.Equal("/api/v4/secrets/{secretName}", definition.Template);
            Assert.True(definition.RequiresAuthorization);
        }

        [Fact]
        public void Get_UniversalAuthLogin_Returns_Definition()
        {
            InfisicalEndpointDefinition definition = InfisicalEndpointRegistry.Get(InfisicalEndpointNames.UniversalAuthLogin);
            Assert.Equal("POST", definition.Method);
            Assert.Equal("v1", definition.Version);
            Assert.Equal("/api/v1/auth/universal-auth/login", definition.Template);
            Assert.False(definition.RequiresAuthorization);
            Assert.True(definition.ContainsSecretMaterialInRequest);
            Assert.True(definition.ContainsSecretMaterialInResponse);
        }

        [Fact]
        public void Get_Unknown_Endpoint_Throws()
        {
            Assert.Throws<InfisicalConfigurationException>(() => InfisicalEndpointRegistry.Get("NotARealEndpoint"));
        }

        [Theory]
        [InlineData(InfisicalEndpointNames.CreateSecret, "POST", "/api/v3/secrets/raw/{secretName}")]
        [InlineData(InfisicalEndpointNames.UpdateSecret, "PATCH", "/api/v3/secrets/raw/{secretName}")]
        [InlineData(InfisicalEndpointNames.DeleteSecret, "DELETE", "/api/v3/secrets/raw/{secretName}")]
        [InlineData(InfisicalEndpointNames.ListProjects, "GET", "/api/v1/workspace")]
        [InlineData(InfisicalEndpointNames.RetrieveProject, "GET", "/api/v1/workspace/{projectId}")]
        [InlineData(InfisicalEndpointNames.CreateProject, "POST", "/api/v2/workspace")]
        [InlineData(InfisicalEndpointNames.UpdateProject, "PATCH", "/api/v1/workspace/{projectId}")]
        [InlineData(InfisicalEndpointNames.DeleteProject, "DELETE", "/api/v1/workspace/{projectId}")]
        [InlineData(InfisicalEndpointNames.CreateEnvironment, "POST", "/api/v1/workspace/{projectId}/environments")]
        [InlineData(InfisicalEndpointNames.UpdateEnvironment, "PATCH", "/api/v1/workspace/{projectId}/environments/{environmentId}")]
        [InlineData(InfisicalEndpointNames.DeleteEnvironment, "DELETE", "/api/v1/workspace/{projectId}/environments/{environmentId}")]
        [InlineData(InfisicalEndpointNames.ListFolders, "GET", "/api/v1/folders")]
        [InlineData(InfisicalEndpointNames.CreateFolder, "POST", "/api/v1/folders")]
        [InlineData(InfisicalEndpointNames.UpdateFolder, "PATCH", "/api/v1/folders/{folderId}")]
        [InlineData(InfisicalEndpointNames.DeleteFolder, "DELETE", "/api/v1/folders/{folderId}")]
        [InlineData(InfisicalEndpointNames.ListTags, "GET", "/api/v1/workspace/{projectId}/tags")]
        [InlineData(InfisicalEndpointNames.CreateTag, "POST", "/api/v1/workspace/{projectId}/tags")]
        [InlineData(InfisicalEndpointNames.UpdateTag, "PATCH", "/api/v1/workspace/{projectId}/tags/{tagId}")]
        [InlineData(InfisicalEndpointNames.DeleteTag, "DELETE", "/api/v1/workspace/{projectId}/tags/{tagId}")]
        [InlineData(InfisicalEndpointNames.JwtAuthLogin, "POST", "/api/v1/auth/jwt-auth/login")]
        [InlineData(InfisicalEndpointNames.OidcAuthLogin, "POST", "/api/v1/auth/oidc-auth/login")]
        [InlineData(InfisicalEndpointNames.LdapAuthLogin, "POST", "/api/v1/auth/ldap-auth/login")]
        [InlineData(InfisicalEndpointNames.AzureAuthLogin, "POST", "/api/v1/auth/azure-auth/login")]
        [InlineData(InfisicalEndpointNames.GcpIamAuthLogin, "POST", "/api/v1/auth/gcp-auth/login")]
        [InlineData(InfisicalEndpointNames.BulkCreateSecret, "POST", "/api/v4/secrets/batch")]
        [InlineData(InfisicalEndpointNames.BulkUpdateSecret, "PATCH", "/api/v4/secrets/batch")]
        [InlineData(InfisicalEndpointNames.BulkDeleteSecret, "DELETE", "/api/v4/secrets/batch")]
        [InlineData(InfisicalEndpointNames.DuplicateSecret, "POST", "/api/v4/secrets/duplicate")]
        public void Registered_Endpoints_Have_Expected_Shape(string name, string method, string template)
        {
            InfisicalEndpointDefinition definition = InfisicalEndpointRegistry.Get(name);
            Assert.Equal(method, definition.Method);
            Assert.Equal(template, definition.Template);
        }

        [Theory]
        [InlineData(InfisicalEndpointNames.BulkCreateSecret, "POST", "/api/v3/secrets/batch/raw")]
        [InlineData(InfisicalEndpointNames.BulkUpdateSecret, "PATCH", "/api/v3/secrets/batch/raw")]
        [InlineData(InfisicalEndpointNames.BulkDeleteSecret, "DELETE", "/api/v3/secrets/batch/raw")]
        public void Bulk_Endpoints_Retain_V3_Fallback_Candidate(string name, string method, string template)
        {
            System.Collections.Generic.IReadOnlyList<InfisicalEndpointDefinition> candidates = InfisicalEndpointRegistry.GetCandidates(name);
            Assert.Equal(2, candidates.Count);
            Assert.Equal("v4", candidates[0].Version);
            Assert.Equal("v3", candidates[1].Version);
            Assert.Equal(method, candidates[1].Method);
            Assert.Equal(template, candidates[1].Template);
        }
    }
}
