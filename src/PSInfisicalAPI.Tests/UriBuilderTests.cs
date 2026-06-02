using System;
using System.Collections.Generic;
using PSInfisicalAPI.Endpoints;
using PSInfisicalAPI.Errors;
using PSInfisicalAPI.Http;
using Xunit;

namespace PSInfisicalAPI.Tests
{
    public class UriBuilderTests
    {
        [Fact]
        public void Build_Combines_Base_And_Endpoint()
        {
            Uri baseUri = new Uri("https://app.infisical.com");
            InfisicalEndpointDefinition definition = InfisicalEndpointRegistry.Get(InfisicalEndpointNames.ListSecrets);

            Uri uri = InfisicalUriBuilder.Build(baseUri, definition, null, new[]
            {
                new KeyValuePair<string, string>("environment", "prod"),
                new KeyValuePair<string, string>("secretPath", "/")
            });

            Assert.Equal("https://app.infisical.com/api/v4/secrets?environment=prod&secretPath=%2F", uri.ToString());
        }

        [Fact]
        public void Build_Escapes_SecretName_Path_Segment()
        {
            InfisicalEndpointDefinition definition = InfisicalEndpointRegistry.Get(InfisicalEndpointNames.RetrieveSecret);
            Dictionary<string, string> pathParameters = new Dictionary<string, string> { { "secretName", "Sql Password" } };

            string resolved = InfisicalUriBuilder.ResolvePathTemplate(definition.Template, pathParameters);

            Assert.Equal("/api/v4/secrets/Sql%20Password", resolved);
        }

        [Fact]
        public void ResolvePathTemplate_Escapes_Slash_And_Colon()
        {
            Dictionary<string, string> pathParameters = new Dictionary<string, string> { { "secretName", "Path/With:Colon" } };
            string resolved = InfisicalUriBuilder.ResolvePathTemplate("/api/v4/secrets/{secretName}", pathParameters);
            Assert.Equal("/api/v4/secrets/Path%2FWith%3AColon", resolved);
        }

        [Fact]
        public void Build_Escapes_Query_Parameters()
        {
            string queryString = InfisicalUriBuilder.BuildQueryString(new[]
            {
                new KeyValuePair<string, string>("tagSlugs", "tag a"),
                new KeyValuePair<string, string>("tagSlugs", "tag/b")
            });

            Assert.Equal("tagSlugs=tag%20a&tagSlugs=tag%2Fb", queryString);
        }

        [Fact]
        public void Build_Throws_When_Required_Token_Missing()
        {
            Uri baseUri = new Uri("https://app.infisical.com");
            InfisicalEndpointDefinition definition = InfisicalEndpointRegistry.Get(InfisicalEndpointNames.RetrieveSecret);
            Assert.Throws<InfisicalConfigurationException>(() => InfisicalUriBuilder.Build(baseUri, definition, null, null));
        }
    }
}
