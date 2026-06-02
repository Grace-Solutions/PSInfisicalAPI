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
    }
}
