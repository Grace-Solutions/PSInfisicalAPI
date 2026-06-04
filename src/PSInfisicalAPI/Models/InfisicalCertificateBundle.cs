namespace PSInfisicalAPI.Models
{
    public sealed class InfisicalCertificateBundle
    {
        public string SerialNumber { get; set; }
        public string CertificatePem { get; set; }
        public string CertificateChainPem { get; set; }
        public string PrivateKeyPem { get; set; }

        public override string ToString()
        {
            return SerialNumber;
        }
    }
}
