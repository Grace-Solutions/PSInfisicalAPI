using System.Collections.Generic;
using PSInfisicalAPI.Errors;

namespace PSInfisicalAPI.Endpoints
{
    public static class InfisicalEndpointRegistry
    {
        private static readonly Dictionary<string, List<InfisicalEndpointDefinition>> Candidates =
            new Dictionary<string, List<InfisicalEndpointDefinition>>
            {
                {
                    InfisicalEndpointNames.UniversalAuthLogin,
                    new List<InfisicalEndpointDefinition>
                    {
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
                    }
                },
                {
                    InfisicalEndpointNames.ListSecrets,
                    new List<InfisicalEndpointDefinition>
                    {
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
                        },
                        new InfisicalEndpointDefinition
                        {
                            Name = InfisicalEndpointNames.ListSecrets,
                            Resource = "Secrets",
                            Version = "v3",
                            Method = "GET",
                            Template = "/api/v3/secrets/raw",
                            RequiresAuthorization = true,
                            ContainsSecretMaterialInRequest = false,
                            ContainsSecretMaterialInResponse = true
                        }
                    }
                },
                {
                    InfisicalEndpointNames.RetrieveSecret,
                    new List<InfisicalEndpointDefinition>
                    {
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
                        },
                        new InfisicalEndpointDefinition
                        {
                            Name = InfisicalEndpointNames.RetrieveSecret,
                            Resource = "Secrets",
                            Version = "v3",
                            Method = "GET",
                            Template = "/api/v3/secrets/raw/{secretName}",
                            RequiresAuthorization = true,
                            ContainsSecretMaterialInRequest = false,
                            ContainsSecretMaterialInResponse = true
                        }
                    }
                }
            };

        public static InfisicalEndpointDefinition Get(string name)
        {
            List<InfisicalEndpointDefinition> list = GetCandidatesInternal(name);
            return list[0];
        }

        public static bool TryGet(string name, out InfisicalEndpointDefinition definition)
        {
            if (string.IsNullOrEmpty(name))
            {
                definition = null;
                return false;
            }

            List<InfisicalEndpointDefinition> list;
            if (!Candidates.TryGetValue(name, out list) || list == null || list.Count == 0)
            {
                definition = null;
                return false;
            }

            definition = list[0];
            return true;
        }

        public static IReadOnlyList<InfisicalEndpointDefinition> GetCandidates(string name)
        {
            return GetCandidatesInternal(name);
        }

        public static IEnumerable<InfisicalEndpointDefinition> All()
        {
            List<InfisicalEndpointDefinition> result = new List<InfisicalEndpointDefinition>();
            foreach (List<InfisicalEndpointDefinition> list in Candidates.Values)
            {
                foreach (InfisicalEndpointDefinition definition in list)
                {
                    result.Add(definition);
                }
            }

            return result;
        }

        private static List<InfisicalEndpointDefinition> GetCandidatesInternal(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new InfisicalConfigurationException("Endpoint name must be provided.");
            }

            List<InfisicalEndpointDefinition> list;
            if (!Candidates.TryGetValue(name, out list) || list == null || list.Count == 0)
            {
                throw new InfisicalConfigurationException(string.Concat("Unknown endpoint name: ", name));
            }

            return list;
        }
    }
}
