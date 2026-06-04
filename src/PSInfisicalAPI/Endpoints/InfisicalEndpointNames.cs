namespace PSInfisicalAPI.Endpoints
{
    public static class InfisicalEndpointNames
    {
        public const string UniversalAuthLogin = "UniversalAuthLogin";
        public const string TokenAuthLogin = "TokenAuthLogin";
        public const string JwtAuthLogin = "JwtAuthLogin";
        public const string OidcAuthLogin = "OidcAuthLogin";
        public const string LdapAuthLogin = "LdapAuthLogin";
        public const string AzureAuthLogin = "AzureAuthLogin";
        public const string GcpIamAuthLogin = "GcpIamAuthLogin";

        public const string ListSecrets = "ListSecrets";
        public const string RetrieveSecret = "RetrieveSecret";
        public const string CreateSecret = "CreateSecret";
        public const string UpdateSecret = "UpdateSecret";
        public const string DeleteSecret = "DeleteSecret";
        public const string BulkCreateSecret = "BulkCreateSecret";
        public const string BulkUpdateSecret = "BulkUpdateSecret";
        public const string BulkDeleteSecret = "BulkDeleteSecret";
        public const string DuplicateSecret = "DuplicateSecret";

        public const string ListProjects = "ListProjects";
        public const string RetrieveProject = "RetrieveProject";
        public const string CreateProject = "CreateProject";
        public const string UpdateProject = "UpdateProject";
        public const string DeleteProject = "DeleteProject";

        public const string ListEnvironments = "ListEnvironments";
        public const string RetrieveEnvironment = "RetrieveEnvironment";
        public const string CreateEnvironment = "CreateEnvironment";
        public const string UpdateEnvironment = "UpdateEnvironment";
        public const string DeleteEnvironment = "DeleteEnvironment";

        public const string ListFolders = "ListFolders";
        public const string RetrieveFolder = "RetrieveFolder";
        public const string CreateFolder = "CreateFolder";
        public const string UpdateFolder = "UpdateFolder";
        public const string DeleteFolder = "DeleteFolder";

        public const string ListTags = "ListTags";
        public const string RetrieveTag = "RetrieveTag";
        public const string CreateTag = "CreateTag";
        public const string UpdateTag = "UpdateTag";
        public const string DeleteTag = "DeleteTag";

        public const string ListInternalCertificateAuthorities = "ListInternalCertificateAuthorities";
        public const string RetrieveInternalCertificateAuthority = "RetrieveInternalCertificateAuthority";
        public const string SearchCertificates = "SearchCertificates";
        public const string RetrieveCertificate = "RetrieveCertificate";
        public const string GetCertificateBundle = "GetCertificateBundle";
        public const string SignCertificateBySubscriber = "SignCertificateBySubscriber";
        public const string SignCertificateByCa = "SignCertificateByCa";

        public const string ListPkiSubscribers = "ListPkiSubscribers";
        public const string GetPkiSubscriber = "GetPkiSubscriber";

        public const string ListCertificateProfiles = "ListCertificateProfiles";
        public const string GetCertificateProfile = "GetCertificateProfile";
    }
}
