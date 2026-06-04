using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace PSInfisicalAPI.Models
{
    public sealed class InfisicalCertificateResult
    {
        public X509Certificate2 Leaf { get; set; }
        public X509Certificate2[] Intermediates { get; set; }
        public X509Certificate2 Root { get; set; }
        public X509Certificate2[] Chain { get; set; }
        public string SerialNumber { get; set; }
        public string CertificatePem { get; set; }
        public string CertificateChainPem { get; set; }
        public string PrivateKeyPem { get; set; }

        public override string ToString()
        {
            if (Leaf != null) { return Leaf.Subject; }
            return SerialNumber;
        }
    }
}
