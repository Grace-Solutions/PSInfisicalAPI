using System;

namespace PSInfisicalAPI.Models
{
    public sealed class InfisicalCertificatePolicy
    {
        public string Id { get; set; }
        public string ProjectId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public InfisicalCertificatePolicySubject Subject { get; set; }
        public InfisicalCertificatePolicySan[] Sans { get; set; }
        public InfisicalCertificatePolicyUsages KeyUsages { get; set; }
        public InfisicalCertificatePolicyUsages ExtendedKeyUsages { get; set; }
        public InfisicalCertificatePolicyAlgorithms Algorithms { get; set; }
        public InfisicalCertificatePolicyValidity Validity { get; set; }
        public DateTimeOffset? CreatedAtUtc { get; set; }
        public DateTimeOffset? UpdatedAtUtc { get; set; }
    }

    public sealed class InfisicalCertificatePolicySubject
    {
        public string Type { get; set; }
        public string[] Allowed { get; set; }
    }

    public sealed class InfisicalCertificatePolicySan
    {
        public string Type { get; set; }
        public string[] Allowed { get; set; }
        public string[] Required { get; set; }
    }

    public sealed class InfisicalCertificatePolicyUsages
    {
        public string[] Allowed { get; set; }
        public string[] Required { get; set; }
    }

    public sealed class InfisicalCertificatePolicyAlgorithms
    {
        public string Signature { get; set; }
        public string[] KeyAlgorithms { get; set; }
    }

    public sealed class InfisicalCertificatePolicyValidity
    {
        public string Max { get; set; }
    }
}
