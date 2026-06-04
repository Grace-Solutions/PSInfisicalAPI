namespace PSInfisicalAPI.Models
{
    public sealed class InfisicalSignedCertificate
    {
        public string SerialNumber { get; set; }
        public string CertificatePem { get; set; }
        public string CertificateChainPem { get; set; }
        public string IssuingCaCertificatePem { get; set; }
        public string PrivateKeyPem { get; set; }
        public string Status { get; set; }
        public string StatusMessage { get; set; }
        public string CertificateRequestId { get; set; }

        public override string ToString()
        {
            return SerialNumber;
        }
    }
}
