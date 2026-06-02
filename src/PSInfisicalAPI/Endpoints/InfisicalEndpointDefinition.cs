namespace PSInfisicalAPI.Endpoints
{
    public sealed class InfisicalEndpointDefinition
    {
        public string Name { get; set; }
        public string Resource { get; set; }
        public string Version { get; set; }
        public string Method { get; set; }
        public string Template { get; set; }
        public bool RequiresAuthorization { get; set; }
        public bool ContainsSecretMaterialInRequest { get; set; }
        public bool ContainsSecretMaterialInResponse { get; set; }
    }
}
