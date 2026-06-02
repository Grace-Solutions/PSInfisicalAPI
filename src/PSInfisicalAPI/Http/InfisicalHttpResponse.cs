using System.Collections.Generic;

namespace PSInfisicalAPI.Http
{
    public sealed class InfisicalHttpResponse
    {
        public int StatusCode { get; set; }
        public string ReasonPhrase { get; set; }
        public string Body { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public bool ContainsSecretMaterialInResponse { get; set; }

        public void Clear()
        {
            Body = null;
            if (Headers != null)
            {
                Headers.Clear();
            }
        }
    }
}
