using System.Collections.Generic;
using PSInfisicalAPI.Errors;

namespace PSInfisicalAPI.Endpoints
{
    public static class InfisicalEndpointRegistry
    {
        private static readonly Dictionary<string, InfisicalEndpointDefinition> Definitions =
            new Dictionary<string, InfisicalEndpointDefinition>
            {
                {
                    InfisicalEndpointNames.UniversalAuthLogin,
                    new InfisicalEndpointDefinition
                    {
                        Name = InfisicalEndpointNames.UniversalAuthLogin,
                        Resource = "Authentication",
                        Version = "v1",
                        Method = "POST",
                        Template = "/api/v1/auth/universal-auth/login",
                        RequiresAuthorization = false,
                        ContainsSecretMaterialInRequest = true,
                        ContainsSecretMaterialInResponse = true
                    }
                },
                {
                    InfisicalEndpointNames.ListSecrets,
                    new InfisicalEndpointDefinition
                    {
                        Name = InfisicalEndpointNames.ListSecrets,
                        Resource = "Secrets",
                        Version = "v4",
                        Method = "GET",
                        Template = "/api/v4/secrets",
                        RequiresAuthorization = true,
                        ContainsSecretMaterialInRequest = false,
                        ContainsSecretMaterialInResponse = true
                    }
                },
                {
                    InfisicalEndpointNames.RetrieveSecret,
                    new InfisicalEndpointDefinition
                    {
                        Name = InfisicalEndpointNames.RetrieveSecret,
                        Resource = "Secrets",
                        Version = "v4",
                        Method = "GET",
                        Template = "/api/v4/secrets/{secretName}",
                        RequiresAuthorization = true,
                        ContainsSecretMaterialInRequest = false,
                        ContainsSecretMaterialInResponse = true
                    }
                }
            };

        public static InfisicalEndpointDefinition Get(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new InfisicalConfigurationException("Endpoint name must be provided.");
            }

            InfisicalEndpointDefinition definition;
            if (!Definitions.TryGetValue(name, out definition))
            {
                throw new InfisicalConfigurationException(string.Concat("Unknown endpoint name: ", name));
            }

            return definition;
        }

        public static bool TryGet(string name, out InfisicalEndpointDefinition definition)
        {
            if (string.IsNullOrEmpty(name))
            {
                definition = null;
                return false;
            }

            return Definitions.TryGetValue(name, out definition);
        }

        public static IEnumerable<InfisicalEndpointDefinition> All()
        {
            return Definitions.Values;
        }
    }
}
