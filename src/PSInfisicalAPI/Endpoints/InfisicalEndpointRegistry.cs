using System.Collections.Generic;
using PSInfisicalAPI.Errors;

namespace PSInfisicalAPI.Endpoints
{
    public static class InfisicalEndpointRegistry
    {
        private static readonly Dictionary<string, List<InfisicalEndpointDefinition>> Candidates;

        static InfisicalEndpointRegistry()
        {
            Candidates = new Dictionary<string, List<InfisicalEndpointDefinition>>();
            RegisterAuthentication(Candidates);
            RegisterSecrets(Candidates);
            RegisterProjects(Candidates);
            RegisterEnvironments(Candidates);
            RegisterFolders(Candidates);
            RegisterTags(Candidates);
            RegisterOrganizations(Candidates);
            RegisterSubOrganizations(Candidates);
            RegisterPki(Candidates);
        }

        private static void Add(Dictionary<string, List<InfisicalEndpointDefinition>> map, InfisicalEndpointDefinition definition)
        {
            List<InfisicalEndpointDefinition> list;
            if (!map.TryGetValue(definition.Name, out list))
            {
                list = new List<InfisicalEndpointDefinition>();
                map[definition.Name] = list;
            }

            list.Add(definition);
        }

        private static void RegisterAuthentication(Dictionary<string, List<InfisicalEndpointDefinition>> map)
        {
            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.UniversalAuthLogin,
                Resource = "Authentication",
                Version = "v1",
                Method = "POST",
                Template = "/api/v1/auth/universal-auth/login",
                RequiresAuthorization = false,
                ContainsSecretMaterialInRequest = true,
                ContainsSecretMaterialInResponse = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.TokenAuthLogin,
                Resource = "Authentication",
                Version = "v1",
                Method = "POST",
                Template = "/api/v1/auth/token-auth/login",
                RequiresAuthorization = false,
                ContainsSecretMaterialInRequest = true,
                ContainsSecretMaterialInResponse = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.JwtAuthLogin,
                Resource = "Authentication",
                Version = "v1",
                Method = "POST",
                Template = "/api/v1/auth/jwt-auth/login",
                RequiresAuthorization = false,
                ContainsSecretMaterialInRequest = true,
                ContainsSecretMaterialInResponse = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.OidcAuthLogin,
                Resource = "Authentication",
                Version = "v1",
                Method = "POST",
                Template = "/api/v1/auth/oidc-auth/login",
                RequiresAuthorization = false,
                ContainsSecretMaterialInRequest = true,
                ContainsSecretMaterialInResponse = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.LdapAuthLogin,
                Resource = "Authentication",
                Version = "v1",
                Method = "POST",
                Template = "/api/v1/auth/ldap-auth/login",
                RequiresAuthorization = false,
                ContainsSecretMaterialInRequest = true,
                ContainsSecretMaterialInResponse = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.AzureAuthLogin,
                Resource = "Authentication",
                Version = "v1",
                Method = "POST",
                Template = "/api/v1/auth/azure-auth/login",
                RequiresAuthorization = false,
                ContainsSecretMaterialInRequest = true,
                ContainsSecretMaterialInResponse = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.GcpIamAuthLogin,
                Resource = "Authentication",
                Version = "v1",
                Method = "POST",
                Template = "/api/v1/auth/gcp-auth/login",
                RequiresAuthorization = false,
                ContainsSecretMaterialInRequest = true,
                ContainsSecretMaterialInResponse = true
            });
        }

        private static void RegisterSecrets(Dictionary<string, List<InfisicalEndpointDefinition>> map)
        {
            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.ListSecrets,
                Resource = "Secrets",
                Version = "v4",
                Method = "GET",
                Template = "/api/v4/secrets",
                RequiresAuthorization = true,
                ContainsSecretMaterialInResponse = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.ListSecrets,
                Resource = "Secrets",
                Version = "v3",
                Method = "GET",
                Template = "/api/v3/secrets/raw",
                RequiresAuthorization = true,
                ContainsSecretMaterialInResponse = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.RetrieveSecret,
                Resource = "Secrets",
                Version = "v4",
                Method = "GET",
                Template = "/api/v4/secrets/{secretName}",
                RequiresAuthorization = true,
                ContainsSecretMaterialInResponse = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.RetrieveSecret,
                Resource = "Secrets",
                Version = "v3",
                Method = "GET",
                Template = "/api/v3/secrets/raw/{secretName}",
                RequiresAuthorization = true,
                ContainsSecretMaterialInResponse = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.CreateSecret,
                Resource = "Secrets",
                Version = "v3",
                Method = "POST",
                Template = "/api/v3/secrets/raw/{secretName}",
                RequiresAuthorization = true,
                ContainsSecretMaterialInRequest = true,
                ContainsSecretMaterialInResponse = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.UpdateSecret,
                Resource = "Secrets",
                Version = "v3",
                Method = "PATCH",
                Template = "/api/v3/secrets/raw/{secretName}",
                RequiresAuthorization = true,
                ContainsSecretMaterialInRequest = true,
                ContainsSecretMaterialInResponse = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.DeleteSecret,
                Resource = "Secrets",
                Version = "v3",
                Method = "DELETE",
                Template = "/api/v3/secrets/raw/{secretName}",
                RequiresAuthorization = true,
                ContainsSecretMaterialInResponse = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.BulkCreateSecret,
                Resource = "Secrets",
                Version = "v4",
                Method = "POST",
                Template = "/api/v4/secrets/batch",
                RequiresAuthorization = true,
                ContainsSecretMaterialInRequest = true,
                ContainsSecretMaterialInResponse = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.BulkCreateSecret,
                Resource = "Secrets",
                Version = "v3",
                Method = "POST",
                Template = "/api/v3/secrets/batch/raw",
                RequiresAuthorization = true,
                ContainsSecretMaterialInRequest = true,
                ContainsSecretMaterialInResponse = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.BulkUpdateSecret,
                Resource = "Secrets",
                Version = "v4",
                Method = "PATCH",
                Template = "/api/v4/secrets/batch",
                RequiresAuthorization = true,
                ContainsSecretMaterialInRequest = true,
                ContainsSecretMaterialInResponse = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.BulkUpdateSecret,
                Resource = "Secrets",
                Version = "v3",
                Method = "PATCH",
                Template = "/api/v3/secrets/batch/raw",
                RequiresAuthorization = true,
                ContainsSecretMaterialInRequest = true,
                ContainsSecretMaterialInResponse = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.BulkDeleteSecret,
                Resource = "Secrets",
                Version = "v4",
                Method = "DELETE",
                Template = "/api/v4/secrets/batch",
                RequiresAuthorization = true,
                ContainsSecretMaterialInRequest = true,
                ContainsSecretMaterialInResponse = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.BulkDeleteSecret,
                Resource = "Secrets",
                Version = "v3",
                Method = "DELETE",
                Template = "/api/v3/secrets/batch/raw",
                RequiresAuthorization = true,
                ContainsSecretMaterialInRequest = true,
                ContainsSecretMaterialInResponse = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.DuplicateSecret,
                Resource = "Secrets",
                Version = "v4",
                Method = "POST",
                Template = "/api/v4/secrets/duplicate",
                RequiresAuthorization = true,
                ContainsSecretMaterialInRequest = true,
                ContainsSecretMaterialInResponse = true
            });
        }

        private static void RegisterProjects(Dictionary<string, List<InfisicalEndpointDefinition>> map)
        {
            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.ListProjects,
                Resource = "Projects",
                Version = "v1",
                Method = "GET",
                Template = "/api/v1/workspace",
                RequiresAuthorization = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.RetrieveProject,
                Resource = "Projects",
                Version = "v1",
                Method = "GET",
                Template = "/api/v1/workspace/{projectId}",
                RequiresAuthorization = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.CreateProject,
                Resource = "Projects",
                Version = "v2",
                Method = "POST",
                Template = "/api/v2/workspace",
                RequiresAuthorization = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.UpdateProject,
                Resource = "Projects",
                Version = "v1",
                Method = "PATCH",
                Template = "/api/v1/workspace/{projectId}",
                RequiresAuthorization = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.DeleteProject,
                Resource = "Projects",
                Version = "v1",
                Method = "DELETE",
                Template = "/api/v1/workspace/{projectId}",
                RequiresAuthorization = true
            });
        }

        private static void RegisterEnvironments(Dictionary<string, List<InfisicalEndpointDefinition>> map)
        {
            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.ListEnvironments,
                Resource = "Environments",
                Version = "v1",
                Method = "GET",
                Template = "/api/v1/workspace/{projectId}",
                RequiresAuthorization = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.RetrieveEnvironment,
                Resource = "Environments",
                Version = "v1",
                Method = "GET",
                Template = "/api/v1/workspace/{projectId}/environments/{environmentId}",
                RequiresAuthorization = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.CreateEnvironment,
                Resource = "Environments",
                Version = "v1",
                Method = "POST",
                Template = "/api/v1/workspace/{projectId}/environments",
                RequiresAuthorization = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.UpdateEnvironment,
                Resource = "Environments",
                Version = "v1",
                Method = "PATCH",
                Template = "/api/v1/workspace/{projectId}/environments/{environmentId}",
                RequiresAuthorization = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.DeleteEnvironment,
                Resource = "Environments",
                Version = "v1",
                Method = "DELETE",
                Template = "/api/v1/workspace/{projectId}/environments/{environmentId}",
                RequiresAuthorization = true
            });
        }

        private static void RegisterFolders(Dictionary<string, List<InfisicalEndpointDefinition>> map)
        {
            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.ListFolders,
                Resource = "Folders",
                Version = "v1",
                Method = "GET",
                Template = "/api/v1/folders",
                RequiresAuthorization = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.RetrieveFolder,
                Resource = "Folders",
                Version = "v1",
                Method = "GET",
                Template = "/api/v1/folders/{folderId}",
                RequiresAuthorization = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.CreateFolder,
                Resource = "Folders",
                Version = "v1",
                Method = "POST",
                Template = "/api/v1/folders",
                RequiresAuthorization = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.UpdateFolder,
                Resource = "Folders",
                Version = "v1",
                Method = "PATCH",
                Template = "/api/v1/folders/{folderId}",
                RequiresAuthorization = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.DeleteFolder,
                Resource = "Folders",
                Version = "v1",
                Method = "DELETE",
                Template = "/api/v1/folders/{folderId}",
                RequiresAuthorization = true
            });
        }

        private static void RegisterTags(Dictionary<string, List<InfisicalEndpointDefinition>> map)
        {
            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.ListTags,
                Resource = "Tags",
                Version = "v1",
                Method = "GET",
                Template = "/api/v1/workspace/{projectId}/tags",
                RequiresAuthorization = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.RetrieveTag,
                Resource = "Tags",
                Version = "v1",
                Method = "GET",
                Template = "/api/v1/workspace/{projectId}/tags/{tagId}",
                RequiresAuthorization = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.CreateTag,
                Resource = "Tags",
                Version = "v1",
                Method = "POST",
                Template = "/api/v1/workspace/{projectId}/tags",
                RequiresAuthorization = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.UpdateTag,
                Resource = "Tags",
                Version = "v1",
                Method = "PATCH",
                Template = "/api/v1/workspace/{projectId}/tags/{tagId}",
                RequiresAuthorization = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.DeleteTag,
                Resource = "Tags",
                Version = "v1",
                Method = "DELETE",
                Template = "/api/v1/workspace/{projectId}/tags/{tagId}",
                RequiresAuthorization = true
            });
        }

        private static void RegisterOrganizations(Dictionary<string, List<InfisicalEndpointDefinition>> map)
        {
            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.ListOrganizations,
                Resource = "Organizations",
                Version = "v2",
                Method = "GET",
                Template = "/api/v2/organizations",
                RequiresAuthorization = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.RetrieveOrganization,
                Resource = "Organizations",
                Version = "v1",
                Method = "GET",
                Template = "/api/v1/organization/{organizationId}",
                RequiresAuthorization = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.CreateOrganization,
                Resource = "Organizations",
                Version = "v2",
                Method = "POST",
                Template = "/api/v2/organizations",
                RequiresAuthorization = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.UpdateOrganization,
                Resource = "Organizations",
                Version = "v1",
                Method = "PATCH",
                Template = "/api/v1/organization/{organizationId}",
                RequiresAuthorization = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.DeleteOrganization,
                Resource = "Organizations",
                Version = "v1",
                Method = "DELETE",
                Template = "/api/v1/organization/{organizationId}",
                RequiresAuthorization = true
            });
        }

        private static void RegisterSubOrganizations(Dictionary<string, List<InfisicalEndpointDefinition>> map)
        {
            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.ListSubOrganizations,
                Resource = "SubOrganizations",
                Version = "v1",
                Method = "GET",
                Template = "/api/v1/sub-organizations",
                RequiresAuthorization = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.RetrieveSubOrganization,
                Resource = "SubOrganizations",
                Version = "v1",
                Method = "GET",
                Template = "/api/v1/sub-organizations/{subOrgId}",
                RequiresAuthorization = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.CreateSubOrganization,
                Resource = "SubOrganizations",
                Version = "v1",
                Method = "POST",
                Template = "/api/v1/sub-organizations",
                RequiresAuthorization = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.UpdateSubOrganization,
                Resource = "SubOrganizations",
                Version = "v1",
                Method = "PATCH",
                Template = "/api/v1/sub-organizations/{subOrgId}",
                RequiresAuthorization = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.DeleteSubOrganization,
                Resource = "SubOrganizations",
                Version = "v1",
                Method = "DELETE",
                Template = "/api/v1/sub-organizations/{subOrgId}",
                RequiresAuthorization = true
            });
        }

        private static void RegisterPki(Dictionary<string, List<InfisicalEndpointDefinition>> map)
        {
            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.ListInternalCertificateAuthorities,
                Resource = "Pki",
                Version = "v1",
                Method = "GET",
                Template = "/api/v1/cert-manager/ca/internal",
                RequiresAuthorization = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.ListInternalCertificateAuthorities,
                Resource = "Pki",
                Version = "v1",
                Method = "GET",
                Template = "/api/v1/pki/ca/internal",
                RequiresAuthorization = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.RetrieveInternalCertificateAuthority,
                Resource = "Pki",
                Version = "v1",
                Method = "GET",
                Template = "/api/v1/cert-manager/ca/internal/{caId}",
                RequiresAuthorization = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.RetrieveInternalCertificateAuthority,
                Resource = "Pki",
                Version = "v1",
                Method = "GET",
                Template = "/api/v1/pki/ca/internal/{caId}",
                RequiresAuthorization = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.SearchCertificates,
                Resource = "Pki",
                Version = "v1",
                Method = "POST",
                Template = "/api/v1/projects/{projectId}/certificates/search",
                RequiresAuthorization = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.RetrieveCertificate,
                Resource = "Pki",
                Version = "v1",
                Method = "GET",
                Template = "/api/v1/pki/certificates/{serialNumber}",
                RequiresAuthorization = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.RetrieveCertificate,
                Resource = "Pki",
                Version = "v1",
                Method = "GET",
                Template = "/api/v1/cert-manager/certificates/{serialNumber}",
                RequiresAuthorization = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.GetCertificateBundle,
                Resource = "Pki",
                Version = "v1",
                Method = "GET",
                Template = "/api/v1/pki/certificates/{serialNumber}/bundle",
                RequiresAuthorization = true,
                ContainsSecretMaterialInResponse = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.GetCertificateBundle,
                Resource = "Pki",
                Version = "v1",
                Method = "GET",
                Template = "/api/v1/cert-manager/certificates/{serialNumber}/bundle",
                RequiresAuthorization = true,
                ContainsSecretMaterialInResponse = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.SignCertificateBySubscriber,
                Resource = "Pki",
                Version = "v1",
                Method = "POST",
                Template = "/api/v1/pki/subscribers/{subscriberName}/sign-certificate",
                RequiresAuthorization = true,
                ContainsSecretMaterialInResponse = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.SignCertificateByCa,
                Resource = "Pki",
                Version = "v1",
                Method = "POST",
                Template = "/api/v1/pki/ca/{caId}/sign-certificate",
                RequiresAuthorization = true,
                ContainsSecretMaterialInResponse = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.SignCertificateByCa,
                Resource = "Pki",
                Version = "v1",
                Method = "POST",
                Template = "/api/v1/cert-manager/ca/{caId}/sign-certificate",
                RequiresAuthorization = true,
                ContainsSecretMaterialInResponse = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.IssueCertificateByProfile,
                Resource = "Pki",
                Version = "v1",
                Method = "POST",
                Template = "/api/v1/cert-manager/certificates",
                RequiresAuthorization = true,
                ContainsSecretMaterialInResponse = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.ListPkiSubscribers,
                Resource = "Pki",
                Version = "v1",
                Method = "GET",
                Template = "/api/v1/projects/{projectId}/pki-subscribers",
                RequiresAuthorization = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.GetPkiSubscriber,
                Resource = "Pki",
                Version = "v1",
                Method = "GET",
                Template = "/api/v1/pki/subscribers/{subscriberName}",
                RequiresAuthorization = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.ListCertificateProfiles,
                Resource = "Pki",
                Version = "v1",
                Method = "GET",
                Template = "/api/v1/cert-manager/certificate-profiles",
                RequiresAuthorization = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.GetCertificateProfile,
                Resource = "Pki",
                Version = "v1",
                Method = "GET",
                Template = "/api/v1/cert-manager/certificate-profiles/{certificateProfileId}",
                RequiresAuthorization = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.ListCertificatePolicies,
                Resource = "Pki",
                Version = "v1",
                Method = "GET",
                Template = "/api/v1/cert-manager/certificate-policies",
                RequiresAuthorization = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.GetCertificatePolicy,
                Resource = "Pki",
                Version = "v1",
                Method = "GET",
                Template = "/api/v1/cert-manager/certificate-policies/{certificatePolicyId}",
                RequiresAuthorization = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.ListCertificateAuthorities,
                Resource = "Pki",
                Version = "v1",
                Method = "GET",
                Template = "/api/v1/cert-manager/ca",
                RequiresAuthorization = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.ListCertificateApplications,
                Resource = "Pki",
                Version = "v1",
                Method = "GET",
                Template = "/api/v1/cert-manager/applications",
                RequiresAuthorization = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.GetCertificateApplication,
                Resource = "Pki",
                Version = "v1",
                Method = "GET",
                Template = "/api/v1/cert-manager/applications/{applicationId}",
                RequiresAuthorization = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.GetCertificateApplicationByName,
                Resource = "Pki",
                Version = "v1",
                Method = "GET",
                Template = "/api/v1/cert-manager/applications/by-name/{name}",
                RequiresAuthorization = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.ListCertificateApplicationProfiles,
                Resource = "Pki",
                Version = "v1",
                Method = "GET",
                Template = "/api/v1/cert-manager/applications/{applicationId}/profiles",
                RequiresAuthorization = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.GetCertificateApplicationEnrollment,
                Resource = "Pki",
                Version = "v1",
                Method = "GET",
                Template = "/api/v1/cert-manager/applications/{applicationId}/profiles/{profileId}/enrollment",
                RequiresAuthorization = true
            });

            Add(map, new InfisicalEndpointDefinition
            {
                Name = InfisicalEndpointNames.GenerateScepDynamicChallenge,
                Resource = "Pki",
                Version = "v1",
                Method = "POST",
                Template = "/scep/applications/{applicationId}/profiles/{profileId}/challenge",
                RequiresAuthorization = true,
                ContainsSecretMaterialInResponse = true
            });
        }

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
