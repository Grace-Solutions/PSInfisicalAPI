using System;
using System.Collections.Generic;

namespace PSInfisicalAPI.Http
{
    public sealed class InfisicalHttpRequest
    {
        public string OperationName { get; set; }
        public string EndpointName { get; set; }
        public string Method { get; set; }
        public Uri Uri { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string Body { get; set; }
        public string ContentType { get; set; }
        public bool ContainsSecretMaterialInRequest { get; set; }
        public bool ContainsSecretMaterialInResponse { get; set; }
    }
}
